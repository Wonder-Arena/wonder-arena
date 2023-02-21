// import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

transaction(name: String, address: Address) {

    let playerCap: Capability<&WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}>
    prepare(acct: AuthAccount) {
        let player <- WonderArenaBattleField_BasicBeasts1.createNewPlayer(
            name: name,
            address: address
        )

        acct.save(<- player, to: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
        self.playerCap = acct.link<&WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}>(
            WonderArenaBattleField_BasicBeasts1.PlayerPublicPath, 
            target: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
            ?? panic("link player failed")
    }

    execute {
        WonderArenaBattleField_BasicBeasts1.register(playerCap: self.playerCap)
    }
}