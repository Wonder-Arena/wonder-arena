import WonderArenaBattleField_BasicBeasts1 from 0x2432e062f9f14295

pub fun main(address: Address): &WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}? {
    if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
        return playerCap.borrow()
    }
    return nil
}