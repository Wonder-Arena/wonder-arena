import WonderArenaBattleField_BasicBeasts1 from "../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(attacker: Address): {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}} {
    if let records = WonderArenaBattleField_BasicBeasts1.attackerChallenges[attacker] {
			return records
    }
    return {}
}