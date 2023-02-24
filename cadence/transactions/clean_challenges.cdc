import WonderArenaBattleField_BasicBeasts1 from "../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

transaction() {
    let adminRef: &WonderArenaBattleField_BasicBeasts1.Admin

    prepare(signer: AuthAccount) {
        self.adminRef = signer
            .borrow<&WonderArenaBattleField_BasicBeasts1.Admin>(from: WonderArenaBattleField_BasicBeasts1.AdminStoragePath)
            ?? panic("borrow battle field admin failed")
    }

    execute {
        self.adminRef.cleanChallenges()
    }
}
