import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

transaction(name: String) {
    prepare(acct: AuthAccount) {
        let player <- WonderArenaBattleField_BasicBeasts1.createNewPlayer(
            name: name,
            address: acct.address
        )

        acct.save(<- player, to: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
        acct.link<&WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}>(
            WonderArenaBattleField_BasicBeasts1.PlayerPublicPath, 
            target: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
            ?? panic("link player failed")
    }

    execute {}
}
