import WonderArenaBattleField_BasicBeasts1 from "../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(attacker: Address): {String: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}}} {
    let res: {String: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}}} = {}
    if let attackRecords = WonderArenaBattleField_BasicBeasts1.attackerChallenges[attacker] {
        res["attack"] = attackRecords
    }

    if let defendRecords = WonderArenaBattleField_BasicBeasts1.defenderChallenges[attacker] {
        res["defend"] = defendRecords
    }
    return res
}