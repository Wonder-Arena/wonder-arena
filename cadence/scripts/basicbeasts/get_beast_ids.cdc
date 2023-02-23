import NonFungibleToken from "../../contracts/core/NonFungibleToken.cdc"
import BasicBeasts from "../../contracts/basicbeasts/BasicBeasts.cdc"

pub fun main(address: Address): [UInt64] {
    let account = getAccount(address)
    let collectionRef = account.getCapability(BasicBeasts.CollectionPublicPath).borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>() 
        ?? panic("Borrow collection failed")

    return collectionRef.getIDs()
}