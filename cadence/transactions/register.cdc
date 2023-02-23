import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

transaction(address: Address) {
    let playerCap: Capability<&WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}>
    let adminRef: &WonderArenaBattleField_BasicBeasts1.Admin

    prepare(signer: AuthAccount) {
        self.playerCap = getAccount(address)
            .getCapability<&WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}>(WonderArenaBattleField_BasicBeasts1.PlayerPublicPath)

        if !self.playerCap.check() {
            panic("empty player capability")
        }

        self.adminRef = signer
            .borrow<&WonderArenaBattleField_BasicBeasts1.Admin>(from: WonderArenaBattleField_BasicBeasts1.AdminStoragePath)
            ?? panic("borrow battle field admin failed")
    }

    execute {
        self.adminRef.register(playerCap: self.playerCap)
    }
}
