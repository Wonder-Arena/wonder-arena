import NonFungibleToken from 0x631e88ae7f1d7c20
import BasicBeasts from 0xfa252d0aa22bf86a

pub fun main(address: Address): [UInt64] {
    let account = getAccount(address)
    let collectionRef = account.getCapability(BasicBeasts.CollectionPublicPath).borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>() 
        ?? panic("Borrow collection failed")

    return collectionRef.getIDs()
}
 