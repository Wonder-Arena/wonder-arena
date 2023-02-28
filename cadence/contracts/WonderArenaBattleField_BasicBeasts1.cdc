
import WonderArenaWorldRules_BasicBeasts1 from "./WonderArenaWorldRules_BasicBeasts1.cdc"
import WonderArenaPawn_BasicBeasts1 from "./WonderArenaPawn_BasicBeasts1.cdc"
import BasicBeasts from "./basicbeasts/BasicBeasts.cdc"

pub contract WonderArenaBattleField_BasicBeasts1 {

    pub let PlayerPublicPath: PublicPath
    pub let PlayerStoragePath: StoragePath
    pub let PlayerPrivatePath: PrivatePath

    pub let AdminStoragePath: StoragePath
    pub let AdminPublicPath: PublicPath
    pub let AdminPrivatePath: PrivatePath

    pub event ContractInitialized()
    pub event PlayerRegistered(name: String, address: Address)
    pub event PlayerUnregistered(name: String, address: Address)
    pub event DefenderGroupAdded(owner: Address, name: String, beasts: [UInt64])
    pub event DefenderGroupUpdated(owner: Address, name: String, beasts: [UInt64])
    pub event DefenderGroupRemoved(owner: Address, name: String, beasts: [UInt64])
    pub event ChallengeHappened(uuid: UInt64, winner: Address, attacker: Address, attackerBeasts: [UInt64], defender: Address, defenderBeasts: [UInt64])

    pub struct BattleEvent {
        // Who trigger this event
	    pub let byBeastID: UInt64?
	    pub let withSkill: String?

	    pub let byStatus: WonderArenaPawn_BasicBeasts1.PawnStatus?

	    pub let targetBeastIDs: [UInt64]

	    // there are some randomness in the game, so we have to record the result
        // What's the result of the event
        pub let hitTheTarget: Bool
	    pub let effect: WonderArenaPawn_BasicBeasts1.PawnEffect?
	    pub let damage: UInt64?

        pub let targetSkipped: Bool
        pub let targetDefeated: Bool
        pub let isAttackSideEffect: Bool

        init(
            byBeastID: UInt64?,
            withSkill: String?,
            byStatus: WonderArenaPawn_BasicBeasts1.PawnStatus?,
            targetBeastIDs: [UInt64],
            hitTheTarget: Bool,
            effect: WonderArenaPawn_BasicBeasts1.PawnEffect?,
            damage: UInt64?,
            targetSkipped: Bool,
            targetDefeated: Bool,
            isAttackSideEffect: Bool
        ) {
            self.byBeastID = byBeastID
            self.withSkill = withSkill
            self.byStatus = byStatus
            self.targetBeastIDs = targetBeastIDs
            self.hitTheTarget = hitTheTarget
            self.effect = effect
            self.damage = damage
            self.targetSkipped = targetSkipped
            self.targetDefeated = targetDefeated
            self.isAttackSideEffect = isAttackSideEffect
        }
    }

    pub struct BeastGroup {
        pub var name: String
        pub let beastIDs: [UInt64]

        init(name: String, beastIDs: [UInt64]) {
            assert(UInt8(beastIDs.length) == WonderArenaWorldRules_BasicBeasts1.groupSize, message: "invalid beasts number")

            let temp: {UInt64: Bool} = {}
            for id in beastIDs {
                temp[id] = true
            }

            assert(temp.keys.length == beastIDs.length, message: "duplicate beasts found")

            self.name = name
            self.beastIDs = beastIDs
        }

        pub fun getID(): String {
            var groupID = ""
            for id in self.beastIDs {
                groupID = groupID.concat(id.toString()).concat("_")
            }
            return groupID
        }
    }

    pub resource interface PlayerPublic {
	    pub var name: String
	    pub let address: Address
        pub fun getDefenderGroups(): [BeastGroup]
    }

    pub struct PlayerStruct {
	    pub let name: String
	    pub let address: Address

        init(
            name: String,
            address: Address
        ) {
            self.name = name
            self.address = address
        }
    }

    pub resource Player: PlayerPublic {
	    pub var name: String
	    pub let address: Address
        pub let defenderGroups: {String: BeastGroup}

        init(name: String, address: Address) {
            self.name = name
            self.address = address
            self.defenderGroups = {}
        }

        pub fun updateName(_ name: String) {
            self.name = name
        }

        pub fun getDefenderGroups(): [BeastGroup] {
            return self.defenderGroups.values
        }

        pub fun addDefenderGroup(group: BeastGroup) {
            let collectionRef = getAccount(self.address)
                .getCapability(BasicBeasts.CollectionPublicPath)
                .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
                ?? panic("Could not borrow beasts colletion of player")

            for id in group.beastIDs {
                if collectionRef.borrowBeast(id: id) == nil {
                    panic("beast is not exist in player's collection")
                }
            }

            let isAlreadyExists = self.defenderGroups[group.name] != nil
            self.defenderGroups[group.name] = group

            // Put th assert here to make sure the group can be kind of `edit`
            let groups = self.getDefenderGroups()
            assert(UInt8(groups.length) <= WonderArenaWorldRules_BasicBeasts1.maxGroupNumber, message: "Exceed max group number")

            if isAlreadyExists {
                emit DefenderGroupUpdated(owner: self.address, name: group.name, beasts: group.beastIDs)
            } else {
                emit DefenderGroupAdded(owner: self.address, name: group.name, beasts: group.beastIDs)
            }
        }

        pub fun removeDefenderGroup(name: String) {
            if let group = self.defenderGroups.remove(key: name) {
                emit DefenderGroupRemoved(owner: self.address, name: group.name, beasts: group.beastIDs)
            }
        }

        destroy() {}
    }

    pub struct ChallengeRecord {
        pub let id: UInt64
	    pub let winner: Address
        pub let attacker: PlayerStruct
	    pub let attackerBeasts: [UInt64]
        pub let defender: PlayerStruct
	    pub let defenderBeasts: [UInt64]
	    pub let events: [BattleEvent]
        pub let attackerScoreChange: Int64
        pub let defenderScoreChange: Int64

        init(
            id: UInt64,
            winner: Address,
            attacker: PlayerStruct,
	        attackerBeasts: [UInt64],
            defender: PlayerStruct,
	        defenderBeasts: [UInt64],
	        events: [BattleEvent],
            attackerScoreChange: Int64,
            defenderScoreChange: Int64
        ) {
            self.id = id
            self.winner = winner
            self.attacker = attacker
            self.attackerBeasts = attackerBeasts
            self.defender = defender
            self.defenderBeasts = defenderBeasts
            self.events = events
            self.attackerScoreChange = attackerScoreChange
            self.defenderScoreChange = defenderScoreChange
        }
    }

    pub var scores: {Address: Int64}
    pub let players: {Address: Capability<&Player{PlayerPublic}>}

    // The records in these to dictionaries should be the same, but
    // we make this so that it can be queried more easily
    // {attacker: {defender: {UUID: ChallengeRecord}}}
    pub var attackerChallenges: {Address: {Address: {UInt64: ChallengeRecord}}}
    // {defender: {attacker: {UUID: ChallengeRecord}}}
    pub var defenderChallenges: {Address: {Address: {UInt64: ChallengeRecord}}}

    pub fun createNewPlayer(name: String, address: Address): @Player {
        let player <- create WonderArenaBattleField_BasicBeasts1.Player(
            name: name,
            address: address
        )

        return <- player
    }

    pub resource Admin {

        pub fun register(playerCap: Capability<&Player{PlayerPublic}>) {
            let player = playerCap.borrow() ?? panic("borrow player failed")
            assert(WonderArenaBattleField_BasicBeasts1.players[player.address] == nil, message: "already registered")

            WonderArenaBattleField_BasicBeasts1.players[player.address] = playerCap
            emit PlayerRegistered(name: player.name, address: player.address)
        }

        pub fun unregister(player: &Player) {
            WonderArenaBattleField_BasicBeasts1.players.remove(key: player.address)
            emit PlayerUnregistered(name: player.name, address: player.address)
        }

        pub fun cleanChallenges() {
            WonderArenaBattleField_BasicBeasts1.attackerChallenges = {}
            WonderArenaBattleField_BasicBeasts1.defenderChallenges = {}
            WonderArenaBattleField_BasicBeasts1.scores = {}
        }

        pub fun fight(attackerAddress: Address, attackerGroup: BeastGroup, defenderAddress: Address) {
            let currentBlockTime = getCurrentBlock().timestamp
            assert(currentBlockTime >= WonderArenaWorldRules_BasicBeasts1.startTime, message: "Game not start yet")
            assert(currentBlockTime <= WonderArenaWorldRules_BasicBeasts1.endTime, message: "Game ended")

            assert(attackerAddress != defenderAddress, message: "attacker and defender should not be the same")
            if let challenges = WonderArenaBattleField_BasicBeasts1.attackerChallenges[attackerAddress] {
                if let _challenges = challenges[defenderAddress] {
                    assert(UInt8(_challenges.keys.length) < WonderArenaWorldRules_BasicBeasts1.maxChallengeTimes, message: "Has challenged for 3 times")
                }
            }

            let attackerPlayerCap = WonderArenaBattleField_BasicBeasts1.players[attackerAddress]
            assert(attackerPlayerCap != nil, message: "attacker is not registered")

            let defenderPlayerCap = WonderArenaBattleField_BasicBeasts1.players[defenderAddress]
            assert(defenderPlayerCap != nil, message: "defender is not registered")

            let attackerPlayer = attackerPlayerCap!.borrow() ?? panic("Could not borrow attacker player")
            let defenderPlayer = defenderPlayerCap!.borrow() ?? panic("Could not borrow defender player")

            let attackerAccount = getAccount(attackerAddress)
            let attackerCollection = attackerAccount
                .getCapability(BasicBeasts.CollectionPublicPath)
                .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
                ?? panic("Borrow attacker pawn collection failed")

            let attackerIDs: [UInt64] = []
            let defenderIDs: [UInt64] = []
            let pawnsMap: {UInt64: WonderArenaPawn_BasicBeasts1.Pawn} = {}
            for id in attackerGroup.beastIDs {
                if let attackerBeast = attackerCollection.borrowBeast(id: id) {
                    attackerIDs.append(id)
                    pawnsMap.insert(key: id, WonderArenaPawn_BasicBeasts1.getPawn(beast: attackerBeast))
                }
            }

            let defenderAccount = getAccount(defenderAddress)
            let defenderCollection = defenderAccount
                .getCapability(BasicBeasts.CollectionPublicPath)
                .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
                ?? panic("Borrow defender pawn collection failed")

            let defenderGroups = defenderPlayer.getDefenderGroups()
            if defenderGroups.length == 0 {
                panic("Defender have 0 defender group")
            } 

            let rand = unsafeRandom()
            let index = rand % UInt64(defenderGroups.length)
            let defenderGroup = defenderGroups[index]
            for id in defenderGroup.beastIDs {
                if let defenderBeast = defenderCollection.borrowBeast(id: id) {
                    defenderIDs.append(id)
                    pawnsMap.insert(key: id, WonderArenaPawn_BasicBeasts1.getPawn(beast: defenderBeast))
                } 
            }

            let counter: UInt8 = 0
            let events: [BattleEvent] = []
            var getWinner = false

            while true {
                if getWinner {
                    break
                }

                let pawns = pawnsMap.values

                let orderedPawns = WonderArenaBattleField_BasicBeasts1.getOrderedPawns(counter: UInt64(counter), pawns: pawns)
                for p in orderedPawns {
                    let isAttacker = attackerIDs.contains(p.nft.id)
                    var opponent = isAttacker ? defenderIDs : attackerIDs

                    let pawn = pawnsMap[p.nft.id]!
                    if pawn.status == WonderArenaPawn_BasicBeasts1.PawnStatus.Defeated {
                        continue
                    }

                    // Pre-attack

                    // Skip
                    var shouldSkip = false
                    if pawn.status == WonderArenaPawn_BasicBeasts1.PawnStatus.Paralysis {
                        let rand = unsafeRandom() % 100
                        if rand < 25 {
                            shouldSkip = true
                        }
                    } else if pawn.status == WonderArenaPawn_BasicBeasts1.PawnStatus.Sleep {
                        shouldSkip = true
                    }

                    if shouldSkip {
                        let skipEvent = BattleEvent(
                            byBeastID: nil,
                            withSkill: nil,
                            byStatus: pawn.status,
                            targetBeastIDs: [pawn.nft.id],
                            hitTheTarget: true,
                            effect: nil,
                            damage: nil,
                            targetSkipped: true,
                            targetDefeated: false,
                            isAttackSideEffect: false
                        )
                        events.append(skipEvent)
                        shouldSkip = true
                    }

                    // Damage
                    if pawn.status == WonderArenaPawn_BasicBeasts1.PawnStatus.Poison {
                        let damage = pawn.maxHp / 10
                        let event = BattleEvent(
                            byBeastID: nil,
                            withSkill: nil,
                            byStatus: pawn.status,
                            targetBeastIDs: [pawn.nft.id],
                            hitTheTarget: true,
                            effect: nil,
                            damage: damage,
                            targetSkipped: false,
                            targetDefeated: false,
                            isAttackSideEffect: false
                        )
                        events.append(event)
                        pawn.setHp(pawn.hp < damage ? 0 : pawn.hp - damage)
                    }

                    // Damage settlement

                    if pawn.hp <= 0 {
                        let defeatedEvent = BattleEvent(
                            byBeastID: nil,
                            withSkill: nil,
                            byStatus: nil,
                            targetBeastIDs: [pawn.nft.id],
                            hitTheTarget: true,
                            effect: nil,
                            damage: nil,
                            targetSkipped: false,
                            targetDefeated: true,
                            isAttackSideEffect: false
                        )
                        events.append(defeatedEvent)
                        pawn.setStatus(WonderArenaPawn_BasicBeasts1.PawnStatus.Defeated)
                        shouldSkip = true
                    }

                    // Cure
                    if pawn.status != WonderArenaPawn_BasicBeasts1.PawnStatus.Defeated {
                        var shouldCure = false
                        if pawn.status == WonderArenaPawn_BasicBeasts1.PawnStatus.Paralysis 
                            || pawn.status == WonderArenaPawn_BasicBeasts1.PawnStatus.Poison {
                            let cureRand = unsafeRandom() % 100
                            if cureRand < 25 {
                                shouldCure = true
                            }
                        } else if pawn.status == WonderArenaPawn_BasicBeasts1.PawnStatus.Sleep {
                            shouldCure = true
                        }

                        if shouldCure {
                            let event = BattleEvent(
                                byBeastID: nil,
                                withSkill: nil,
                                byStatus: pawn.status,
                                targetBeastIDs: [pawn.nft.id],
                                hitTheTarget: true,
                                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToNormal,
                                damage: nil,
                                targetSkipped: false,
                                targetDefeated: false,
                                isAttackSideEffect: false
                            )
                            events.append(event)
                            pawn.setStatus(WonderArenaPawn_BasicBeasts1.PawnStatus.Normal)
                        }
                    }

                    if !shouldSkip && pawn.status != WonderArenaPawn_BasicBeasts1.PawnStatus.Defeated {
                        var _target: WonderArenaPawn_BasicBeasts1.Pawn? = nil
                        for id in opponent {
                            if pawnsMap[id]!.hp != 0 {
                                _target = pawnsMap[id]
                                break
                            }
                        }

                        // Attack
                        if let target = _target {
                            var hitTheTarget = false
                            let attackRand = unsafeRandom() % 100
                            if attackRand < pawn.accuracy {
                                hitTheTarget = true
                            }

                            if hitTheTarget {
                                let multiplier = WonderArenaBattleField_BasicBeasts1.getTypeMultiplier(attackerType: pawn.type, defenderType: target.type)
                                var skill: String? = nil
                                var attackValue = pawn.attack.value
                                if pawn.mana >= pawn.skill.manaRequired {
                                    skill = pawn.skill.name
                                    attackValue = pawn.skill.value
                                    pawn.setMana(pawn.mana - pawn.skill.manaRequired)
                                }

                                let rawDamage = (attackValue * multiplier) / 100
                                let damage: UInt64 = rawDamage > target.defense ? rawDamage - target.defense : 0
                                let event = BattleEvent(
                                    byBeastID: pawn.nft.id,
                                    withSkill: skill,
                                    byStatus: nil,
                                    targetBeastIDs: [target.nft.id],
                                    hitTheTarget: true,
                                    effect: nil,
                                    damage: damage,
                                    targetSkipped: false,
                                    targetDefeated: false,
                                    isAttackSideEffect: false
                                )
                                events.append(event)
                                target.setHp(target.hp < damage ? 0 : target.hp - damage)
                                target.setMana(target.mana + damage)

                                if pawn.attack.effect != WonderArenaPawn_BasicBeasts1.PawnEffect.None {
                                    let effectRand = unsafeRandom() % 100
                                    var effect: WonderArenaPawn_BasicBeasts1.PawnEffect? = nil
                                    if effectRand < pawn.attack.effectProb {
                                        effect = pawn.attack.effect
                                        let event = BattleEvent(
                                            byBeastID: pawn.nft.id,
                                            withSkill: nil,
                                            byStatus: nil,
                                            targetBeastIDs: [target.nft.id],
                                            hitTheTarget: true,
                                            effect: effect,
                                            damage: nil,
                                            targetSkipped: false,
                                            targetDefeated: false,
                                            isAttackSideEffect: true
                                        )
                                        events.append(event)
                                        target.setStatus(WonderArenaPawn_BasicBeasts1.effectToStatus(effect: effect!))
                                    }
                                }

                                if target.hp <= 0 {
                                    let defeatedEvent = BattleEvent(
                                        byBeastID: nil,
                                        withSkill: nil,
                                        byStatus: nil,
                                        targetBeastIDs: [target.nft.id],
                                        hitTheTarget: true,
                                        effect: nil,
                                        damage: nil,
                                        targetSkipped: false,
                                        targetDefeated: true,
                                        isAttackSideEffect: false
                                    )
                                    events.append(defeatedEvent)
                                    target.setStatus(WonderArenaPawn_BasicBeasts1.PawnStatus.Defeated)
                                }

                                pawnsMap[target.nft.id] = target
                            }
                        }
                    }

                    pawnsMap[pawn.nft.id] = pawn

                    // Winner

                    var winner: Address? = nil  
                    var atLeastOneAttackerAlive = false
                    for id in attackerIDs {
                        if pawnsMap[id]!.hp != 0 {
                            atLeastOneAttackerAlive = true
                            break
                        }
                    }

                    var atLeastOneDefenderAlive = false
                    for id in defenderIDs {
                        if pawnsMap[id]!.hp != 0 {
                            atLeastOneDefenderAlive = true
                            break
                        }
                    }

                    if !atLeastOneAttackerAlive {
                        winner = defenderAddress
                    } else if !atLeastOneDefenderAlive {
                        winner = attackerAddress
                    }

                    if let winnerAddress = winner {
                        self.addRecord(
                            winnerAddress: winnerAddress,
                            attackerAddress: attackerAddress,
                            attackerBeasts: attackerIDs,
                            defenderAddress: defenderAddress,
                            defenderBeasts: defenderIDs,
                            events: events
                        )
                        getWinner = true
                        break
                    }
                }
            }
        }

        pub fun addRecord (
            winnerAddress: Address,
            attackerAddress: Address, 
            attackerBeasts: [UInt64], 
            defenderAddress: Address,
            defenderBeasts: [UInt64], 
            events: [BattleEvent]
        ) {
            let attackerScoreChange: Int64 = winnerAddress == attackerAddress ? 60 : -30
            let defenderScoreChange: Int64 = winnerAddress == attackerAddress ? -30 : 60
            let recordUUID = WonderArenaBattleField_BasicBeasts1.generateUUID()

            let attackerPlayer = (WonderArenaBattleField_BasicBeasts1.players[attackerAddress]!).borrow() ?? panic("Attacker is not exist")
            let defenderPlayer = (WonderArenaBattleField_BasicBeasts1.players[defenderAddress]!).borrow() ?? panic("Defender is not exist")

            let record = ChallengeRecord(
                id: recordUUID,
                winner: winnerAddress,
                attacker: PlayerStruct(name: attackerPlayer.name, address: attackerPlayer.address),
                attackerBeasts: attackerBeasts,
                defender: PlayerStruct(name: defenderPlayer.name, address: defenderPlayer.address),
                defenderBeasts: defenderBeasts,
                events: events,
                attackerScoreChange: attackerScoreChange,
                defenderScoreChange: defenderScoreChange
            )

            if let attackerScore = WonderArenaBattleField_BasicBeasts1.scores[attackerAddress] {
                var newScore = attackerScore + attackerScoreChange
                if newScore < 0 {
                    newScore = 0
                }
                WonderArenaBattleField_BasicBeasts1.scores[attackerAddress] = newScore
            } else {
                WonderArenaBattleField_BasicBeasts1.scores[attackerAddress] = attackerScoreChange > 0 ? attackerScoreChange : 0
            }

            if let defenderScore = WonderArenaBattleField_BasicBeasts1.scores[defenderAddress] {
                var newScore = defenderScore + defenderScoreChange
                if newScore < 0 {
                    newScore = 0
                }
                WonderArenaBattleField_BasicBeasts1.scores[defenderAddress] = newScore
            } else {
                WonderArenaBattleField_BasicBeasts1.scores[defenderAddress] = defenderScoreChange > 0 ? defenderScoreChange : 0
            }

            var attackerChallenges = WonderArenaBattleField_BasicBeasts1.attackerChallenges[attackerAddress]
            if attackerChallenges == nil {
                attackerChallenges = {}
            }

            var aRecords = attackerChallenges![defenderAddress]
            if aRecords == nil {
                aRecords = {}
            }
            aRecords!.insert(key: recordUUID, record)
            attackerChallenges!.insert(key: defenderAddress, aRecords!)
            WonderArenaBattleField_BasicBeasts1.attackerChallenges.insert(key: attackerAddress, attackerChallenges!)

            var defenderChallenges = WonderArenaBattleField_BasicBeasts1.defenderChallenges[defenderAddress]
            if defenderChallenges == nil {
                defenderChallenges = {}
            }

            var dRecords = defenderChallenges![attackerAddress]
            if dRecords == nil {
                dRecords = {}
            }
            dRecords!.insert(key: recordUUID, record)
            defenderChallenges!.insert(key: attackerAddress, dRecords!)
            WonderArenaBattleField_BasicBeasts1.defenderChallenges.insert(key: defenderAddress, defenderChallenges!)

            emit ChallengeHappened(
                uuid: recordUUID,
                winner: winnerAddress, 
                attacker: attackerAddress, 
                attackerBeasts: attackerBeasts, 
                defender: defenderAddress, 
                defenderBeasts: defenderBeasts
            )
        } 
    }

    pub fun getTypeMultiplier(
        attackerType: WonderArenaPawn_BasicBeasts1.PawnType,
        defenderType: WonderArenaPawn_BasicBeasts1.PawnType
    ): UInt64 {
        let map: {WonderArenaPawn_BasicBeasts1.PawnType: {WonderArenaPawn_BasicBeasts1.PawnType: UInt64}} = {
            WonderArenaPawn_BasicBeasts1.PawnType.Normal: {
                WonderArenaPawn_BasicBeasts1.PawnType.Normal: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Fire: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Water: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Grass: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Electric: 100
            },
            WonderArenaPawn_BasicBeasts1.PawnType.Fire: {
                WonderArenaPawn_BasicBeasts1.PawnType.Normal: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Fire: 50,
                WonderArenaPawn_BasicBeasts1.PawnType.Water: 50,
                WonderArenaPawn_BasicBeasts1.PawnType.Grass: 200,
                WonderArenaPawn_BasicBeasts1.PawnType.Electric: 100
            },
            WonderArenaPawn_BasicBeasts1.PawnType.Water: {
                WonderArenaPawn_BasicBeasts1.PawnType.Normal: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Fire: 200,
                WonderArenaPawn_BasicBeasts1.PawnType.Water: 50,
                WonderArenaPawn_BasicBeasts1.PawnType.Grass: 50,
                WonderArenaPawn_BasicBeasts1.PawnType.Electric: 100
            },
            WonderArenaPawn_BasicBeasts1.PawnType.Grass: {
                WonderArenaPawn_BasicBeasts1.PawnType.Normal: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Fire: 50,
                WonderArenaPawn_BasicBeasts1.PawnType.Water: 200,
                WonderArenaPawn_BasicBeasts1.PawnType.Grass: 50,
                WonderArenaPawn_BasicBeasts1.PawnType.Electric: 100
            },
            WonderArenaPawn_BasicBeasts1.PawnType.Electric: {
                WonderArenaPawn_BasicBeasts1.PawnType.Normal: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Fire: 100,
                WonderArenaPawn_BasicBeasts1.PawnType.Water: 200,
                WonderArenaPawn_BasicBeasts1.PawnType.Grass: 50,
                WonderArenaPawn_BasicBeasts1.PawnType.Electric: 50
            }
        }
        return map[attackerType]![defenderType]!
    }

    pub resource EmptyResource {
        init() {}
    }

    pub fun generateUUID(): UInt64 {
        let r <- create EmptyResource()
        let uuid = r.uuid
        destroy r
        return uuid
    }

    pub fun getOrderedPawns(counter: UInt64, pawns: [WonderArenaPawn_BasicBeasts1.Pawn]): [WonderArenaPawn_BasicBeasts1.Pawn] {
        var swapped = true

        while swapped {
            swapped = false
            var va = 0
            while va < pawns.length - 1 {
                let pawn1 = pawns[va]
                let pawn2 = pawns[va+1]
                if (pawn1.agility + counter * pawn1.speed > pawn2.agility + counter * pawn2.speed) {
                    let swap = pawn1
                    pawns[va] = pawn2
                    pawns[va+1] = swap
                    swapped = true
                }
                va = va + 1
            }
        }

        return pawns
    }

    init() {
        self.PlayerStoragePath = /storage/WonderArenaBattleField_Player_BasicBeasts1
        self.PlayerPublicPath = /public/WonderArenaBattleField_Player_BasicBeasts1
        self.PlayerPrivatePath = /private/WonderArenaBattleField_Player_BasicBeasts1

        self.AdminStoragePath = /storage/WonderArenaBattleField_Admin_BasicBeasts1
        self.AdminPublicPath = /public/WonderArenaBattleField_Admin_BasicBeasts1
        self.AdminPrivatePath = /private/WonderArenaBattleField_Admin_BasicBeasts1

        self.account.save(<- create Admin(), to: self.AdminStoragePath)

        self.scores = {}
        self.players = {}
        self.attackerChallenges = {}
        self.defenderChallenges = {}

        emit ContractInitialized()
    }
}