import WonderArenaBattleField_BasicBeasts1 from 0x469d7a2394a488bb

pub fun main(attacker: Address, defender: Address, uuid: UInt64): WonderArenaBattleField_BasicBeasts1.ChallengeRecord? {
    if let records = WonderArenaBattleField_BasicBeasts1.attackerChallenges[attacker] {
        if let innerRecords = records[defender] {
			return innerRecords[uuid] 
        }
    }
    return nil
}
