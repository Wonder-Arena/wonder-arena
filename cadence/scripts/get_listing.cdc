import NonFungibleToken from "../contracts/core/NonFungibleToken.cdc"
import BasicBeasts from "../contracts/basicbeasts/BasicBeasts.cdc"

pub fun main(): {UInt64: &BasicBeasts.NFT{BasicBeasts.Public}} {
    let collection = getAccount(0x2432e062f9f14295)
        .getCapability(BasicBeasts.CollectionPublicPath)
        .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
        ?? panic("Could not borrow seller collection")

    let tokenIDs = collection.getIDs()
    let res: {UInt64: &BasicBeasts.NFT{BasicBeasts.Public}} = {}
    var counter = 0
    for id in tokenIDs {
        if let beast = collection.borrowBeast(id: id) {
            res[id] = beast
        }

        if res.keys.length >= 5 {
            break
        }
    }

    return res
}