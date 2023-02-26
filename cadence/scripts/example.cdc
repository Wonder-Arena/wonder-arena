pub enum PawnEffect: UInt8 {
    pub case None
    pub case ToNormal
    pub case ToParalysis
    pub case ToPoison
    pub case ToSleep
}

pub enum PawnStatus: UInt8 {
    pub case Normal
    pub case Paralysis
    pub case Poison
    pub case Sleep
    pub case Defeated
}

pub struct BattleEvent {
    // Who trigger this event
    pub let byBeastID: UInt64?
    pub let byStatus: PawnStatus?

    pub let withSkill: String?
    pub let targetBeastIDs: [UInt64]

    // there are some randomness in the game, so we have to record the result
    // What's the result of the event
    pub let hitTheTarget: Bool
    pub let effect: PawnEffect?
    pub let damage: UInt64?

    pub let targetSkipped: Bool
    pub let targetDefeated: Bool

    init(
        byBeastID: UInt64?,
        byStatus: PawnStatus?,
        withSkill: String?,
        targetBeastIDs: [UInt64],
        hitTheTarget: Bool,
        effect: PawnEffect?,
        damage: UInt64?,
        targetSkipped: Bool,
        targetDefeated: Bool
    ) {
        self.byBeastID = byBeastID
        self.byStatus = byStatus
        self.withSkill = withSkill
        self.targetBeastIDs = targetBeastIDs
        self.hitTheTarget = hitTheTarget
        self.effect = effect
        self.damage = damage
        self.targetSkipped = targetSkipped
        self.targetDefeated = targetDefeated
    }
}

pub struct Beast {
    pub let id: UInt64
    pub var hp: UInt64
    pub var status: PawnStatus

    init(id: UInt64, hp: UInt64, status: PawnStatus) {
        self.id = id
        self.hp = hp
        self.status = status
    }

    pub fun setHP(_ hp: UInt64) {
        self.hp = hp
    }

    pub fun setStatus(_ status: PawnStatus) {
        self.status = status
    }
}

pub fun main() {
  
  let events: [BattleEvent] = [
    // 123's turn start
    // 123 attack 321 with normal attack, and make 30 final damage
    BattleEvent(
        byBeastID: 123,
        byStatus: nil,
        withSkill: nil,
        targetBeastIDs: [321],
        hitTheTarget: true,
        effect: nil,
        damage: 30,
        targetSkipped: false,
        targetDefeated: false
    ),
    // 123's attack also cause 321 to Poison
    BattleEvent(
        byBeastID: 123,
        byStatus: nil,
        withSkill: nil,
        targetBeastIDs: [321],
        hitTheTarget: true,
        effect: PawnEffect.ToPoison,
        damage: nil,
        targetSkipped: false,
        targetDefeated: false
    ),
    // 👆 are the events triggered by 123's attack
    // there can have multiple events be triggered in a turn

    // 321's turn start
    // Due to the Poison status, 321 suffered 10 damage
    BattleEvent(
        byBeastID: nil,
        byStatus: PawnStatus.Poison,
        withSkill: nil,
        targetBeastIDs: [321],
        hitTheTarget: true,
        effect: nil,
        damage: 10,
        targetSkipped: false,
        targetDefeated: false
    ),
    // 321 attack 123 with normal attack, cause 20 damage
    BattleEvent(
        byBeastID: 321,
        byStatus: nil,
        withSkill: nil,
        targetBeastIDs: [123],
        hitTheTarget: true,
        effect: nil,
        damage: 20,
        targetSkipped: false,
        targetDefeated: false
    ),
    
    // 123's turn, 123 using Ultimate Skill named "Frenzy"
    // caused 40 damage to 321
    BattleEvent(
        byBeastID: 123,
        byStatus: nil,
        withSkill: "Frenzy",
        targetBeastIDs: [321],
        hitTheTarget: true,
        effect: nil,
        damage: 40,
        targetSkipped: false,
        targetDefeated: false
    ),
    // 321's turn
    // 321 suffered 10 damage due to Poison
    BattleEvent(
        byBeastID: nil,
        byStatus: PawnStatus.Poison,
        withSkill: nil,
        targetBeastIDs: [321],
        hitTheTarget: true,
        effect: nil,
        damage: 10,
        targetSkipped: false,
        targetDefeated: false
    ),
    // 321 cured and status will be normal
    BattleEvent(
        byBeastID: nil,
        byStatus: nil,
        withSkill: nil,
        targetBeastIDs: [321],
        hitTheTarget: true,
        effect: PawnEffect.ToNormal,
        damage: nil,
        targetSkipped: false,
        targetDefeated: false
    ),
    // 321 attack 123 with Ultimate Skill, caused 100 damage
    BattleEvent(
        byBeastID: 321,
        byStatus: nil,
        withSkill: "Super Punch",
        targetBeastIDs: [123],
        hitTheTarget: true,
        effect: nil,
        damage: 100,
        targetSkipped: false,
        targetDefeated: false
    ),
    // 123 defeated
    BattleEvent(
        byBeastID: nil,
        byStatus: nil,
        withSkill: nil,
        targetBeastIDs: [123],
        hitTheTarget: true,
        effect: nil,
        damage: nil,
        targetSkipped: false,
        targetDefeated: true 
    )
  ]

  // Get all unique beasts by BeastID (note: also need to check targetBeastIDs, but this is just an example)
  let beasts: {UInt64: Beast} = {
    123: Beast(id: 123, hp: 120, status: PawnStatus.Normal),
    321: Beast(id: 321, hp: 120, status: PawnStatus.Normal)
  }

  // Read all events
  let b123: &Beast = &(beasts[123]!) as &Beast
  let b321: &Beast = &(beasts[321]!) as &Beast

  for e in events {
    if let attacker = e.byBeastID {
        let target = e.targetBeastIDs[0] == 123 ? b123 : b321
        let isSideEffect = e.effect != nil

        if let skill = e.withSkill {
            if !isSideEffect {
                log(attacker.toString().concat("'s attack ").concat(target.id.toString()).concat(" by using ").concat(skill))
            }
        } else {
            if !isSideEffect {
                log(attacker.toString().concat(" attack ").concat(target.id.toString()))
            }
        }

        if !e.hitTheTarget && !isSideEffect {
            log("MISS!")
        } else {
            if let damage = e.damage {
                log(target.id.toString().concat(" suffered ").concat(damage.toString()).concat(" damage!"))
                target.setHP(target.hp - damage > 0 ? target.hp - damage : 0)
                log(target.id.toString().concat(" HP: ".concat(target.hp.toString())))
            } else if let effect = e.effect {
                if effect == PawnEffect.ToPoison {
                    log(target.id.toString().concat(" Poisoned!"))
                    target.setStatus(PawnStatus.Poison)
                    log(target.id.toString().concat(" Status: Poison"))
                } else if effect == PawnEffect.ToParalysis {
                    log(target.id.toString().concat(" is paralysed!"))
                    log(target.id.toString().concat(" Status: Paralysis"))
                    target.setStatus(PawnStatus.Paralysis)
                } else if effect == PawnEffect.ToSleep {
                    log(target.id.toString().concat(" is sleeping!"))
                    log(target.id.toString().concat(" Status: Sleep"))
                    target.setStatus(PawnStatus.Sleep)
                } else if effect == PawnEffect.ToNormal {
                    log(target.id.toString().concat(" now is normal!"))
                    log(target.id.toString().concat(" Status: Normal"))
                    target.setStatus(PawnStatus.Normal)
                } 
            }
        }

    } else if let status = e.byStatus {
        let target = e.targetBeastIDs[0] == 123 ? b123 : b321
        var suffix = ""
        if status == PawnStatus.Poison {
            suffix = " due to Poison status"
        } else if status == PawnStatus.Paralysis {
            suffix = " due to Paralysis status"
        } else if status == PawnStatus.Sleep {
            suffix = " due to Sleep status"
        }

        if e.hitTheTarget {
            if let damage = e.damage {
                let statement = target.id.toString().concat(" suffered ").concat(damage.toString()).concat(" damage")
                log(statement.concat(suffix))
                target.setHP(target.hp - damage > 0 ? target.hp - damage : 0) 
                log(target.id.toString().concat(" HP: ".concat(target.hp.toString())))
            } else if e.targetSkipped {
                log(target.id.toString().concat("'s turn is skipped").concat(suffix))
            } else if let effect = e.effect {
                // Only ToNormal will be happen when the event is triggered by status
                if effect == PawnEffect.ToNormal {
                    log(target.id.toString().concat(" now is normal!"))
                    log(target.id.toString().concat(" Status: Normal"))
                    target.setStatus(PawnStatus.Normal)
                } 
            }
        }
    } else if e.targetDefeated {
        let target = e.targetBeastIDs[0] == 123 ? b123 : b321
        log(target.id.toString().concat(" is defeated"))
    }
  }
}