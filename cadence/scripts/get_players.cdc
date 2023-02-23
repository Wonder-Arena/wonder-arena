import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(): [&{WonderArenaBattleField_BasicBeasts1.PlayerPublic}] {
    let playerCaps = WonderArenaBattleField_BasicBeasts1.players.values

    let res: [&{WonderArenaBattleField_BasicBeasts1.PlayerPublic}] = []
    for cap in playerCaps {
        if let player = cap.borrow() {
            res.append(player)
        }
    }
    return res
}