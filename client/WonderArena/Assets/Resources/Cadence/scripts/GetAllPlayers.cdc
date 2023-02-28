import WonderArenaBattleField_BasicBeasts1 from 0x469d7a2394a488bb

pub struct Player {
    pub let name: String
    pub let address: Address
    pub let score: Int64
    pub let defenderGroups: [WonderArenaBattleField_BasicBeasts1.BeastGroup]
    pub let isChallengable: Bool

    init(
        name: String,
        address: Address,
        score: Int64,
        defenderGroups: [WonderArenaBattleField_BasicBeasts1.BeastGroup],
        isChallengable: Bool
    ) {
        self.name = name
        self.address = address
        self.score = score
        self.defenderGroups = defenderGroups
        self.isChallengable = isChallengable
    }
}

pub fun main(): {Address: Player} {
    let res: {Address: Player} = {}
    for address in WonderArenaBattleField_BasicBeasts1.players.keys {
        if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
            if let _player = playerCap.borrow() {
                let defenderGroups = _player.getDefenderGroups()
                let player = Player(
                    name: _player.name,
                    address: address,
                    score: WonderArenaBattleField_BasicBeasts1.scores[address] ?? 0,
                    defenderGroups: defenderGroups,
                    isChallengable: defenderGroups.length > 0
                )
                res[address] = player
            }
        }
    }
    return res
}