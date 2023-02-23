import WonderArenaPawn_BasicBeasts1 from "../contracts/WonderArenaPawn_BasicBeasts1.cdc"
import BasicBeasts from "../contracts/basicbeasts/BasicBeasts.cdc"

pub fun main(address: Address, beastIDs: [UInt64]): [WonderArenaPawn_BasicBeasts1.Pawn] {
    let account = getAccount(address)
    let collectionRef = account.getCapability(BasicBeasts.CollectionPublicPath).borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>() 
        ?? panic("Borrow collection failed")

    let res: [WonderArenaPawn_BasicBeasts1.Pawn] = []
    for id in beastIDs {
        if let beast = collectionRef.borrowBeast(id: id) {
            let pawn = WonderArenaPawn_BasicBeasts1.getPawn(beast: beast)
            res.append(pawn)
        }
    }
    return res
}