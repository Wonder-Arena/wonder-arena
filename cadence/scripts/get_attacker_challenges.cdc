import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(attacker: Address, defender: Address): [WonderArenaBattleField_BasicBeasts1.ChallengeRecord] {
    if let attackerRecords = WonderArenaBattleField_BasicBeasts1.attackerChallenges[attacker] {
        if let records = attackerRecords[defender] {
            return records
        }
    }
    return []
}