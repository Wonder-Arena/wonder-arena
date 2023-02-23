import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(addresses: [Address]): {Address: Int64} {
    let scores: {Address: Int64} = {}
    for address in addresses {
        if let score = WonderArenaBattleField_BasicBeasts1.scores[address] {
            scores.insert(key: address, score)
        } else {
            scores.insert(key: address, 0)
        }
    }
    return scores
}