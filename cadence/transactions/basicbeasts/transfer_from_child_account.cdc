import ChildAccount from "../../contracts/core/ChildAccount.cdc"
import BasicBeasts from "../../contracts/basicbeasts/BasicBeasts.cdc"

transaction(childAddress: Address, tokenIDs: [UInt64]) {
    let childCollection: &BasicBeasts.Collection
    let parentCollection: &BasicBeasts.Collection

    prepare(acct: AuthAccount) {
        let managerRef = acct
            .borrow<&ChildAccount.ChildAccountManager>(from: ChildAccount.ChildAccountManagerStoragePath)
            ?? panic("borrow child account manager failed")

        let childAccountRef = managerRef.getChildAccountRef(address: childAddress) 
            ?? panic("get child account ref failed")

        self.childCollection = childAccountRef
            .borrow<&BasicBeasts.Collection>(from: BasicBeasts.CollectionStoragePath)
            ?? panic("borrow child collection failed")

        self.parentCollection = acct
            .borrow<&BasicBeasts.Collection>(from: BasicBeasts.CollectionStoragePath)
            ?? panic("borrow parent collection failed")
    }

    execute {
        for id in tokenIDs {
            let beast <- self.childCollection.withdraw(withdrawID: id)
            self.parentCollection.deposit(token: <- beast)
        }
    }
}