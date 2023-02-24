import WonderArenaBattleField_BasicBeasts1 from "../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub struct Player {
    pub let name: String
    pub let address: Address
    pub let score: Int64

    init(
        name: String,
        address: Address,
        score: Int64
    ) {
        self.name = name
        self.address = address
        self.score = score
    }
}

pub fun main(): {Address: Player} {
    let scores = WonderArenaBattleField_BasicBeasts1.scores
    let res: {Address: Player} = {}
    for address in scores.keys {
        if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
            if let _player = playerCap.borrow() {
                let score = scores[address]!
                let player = Player(
                    name: _player.name,
                    address: address,
                    score: score
                )
                res[address] = player
            }
        }
    }
    return res
}