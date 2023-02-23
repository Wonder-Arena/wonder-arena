import NonFungibleToken from 0x631e88ae7f1d7c20
import BasicBeasts from 0xfa252d0aa22bf86a

transaction(tokenIDs: [UInt64], recipient: Address) {
    let senderCollection: &BasicBeasts.Collection
    let recipientCollection: &BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}

    prepare(acct: AuthAccount) {
        self.senderCollection = acct.borrow<&BasicBeasts.Collection>(from: BasicBeasts.CollectionStoragePath)
            ?? panic("borrow sender collection failed")

        self.recipientCollection = getAccount(recipient)
            .getCapability(BasicBeasts.CollectionPublicPath)
            .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
            ?? panic("borrow recipient collection failed")
    }

    execute {
        for id in tokenIDs {
            let beast <- self.senderCollection.withdraw(withdrawID: id)
            self.recipientCollection.deposit(token: <- beast)
        }
    }
}