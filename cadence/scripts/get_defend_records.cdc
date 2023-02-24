import WonderArenaBattleField_BasicBeasts1 from "../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(defender: Address): {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}} {
    if let records = WonderArenaBattleField_BasicBeasts1.defenderChallenges[defender] {
		return records
    }
    return {}
}