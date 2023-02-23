import NonFungibleToken from "../../contracts/core/NonFungibleToken.cdc"
import BasicBeasts from "../../contracts/basicbeasts/BasicBeasts.cdc"

transaction(templateID: UInt32, recipient: Address) {
    let adminRef: &BasicBeasts.Admin
    let collectionRef: &BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}
    prepare(acct: AuthAccount) {
        self.adminRef = acct.borrow<&BasicBeasts.Admin>(from: BasicBeasts.AdminStoragePath)
            ?? panic("Borrow admin failed")

        let recipientAcct = getAccount(recipient)
        self.collectionRef = recipientAcct.getCapability(BasicBeasts.CollectionPublicPath).borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>() 
            ?? panic("Borrow collection failed")
    }

    execute {
        let beast <- self.adminRef.mintBeast(beastTemplateID: templateID)
        self.collectionRef.deposit(token: <- beast)
    }
}
