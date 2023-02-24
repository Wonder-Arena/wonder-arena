import WonderArenaPawn_BasicBeasts1 from 0x2432e062f9f14295
import BasicBeasts from 0xfa252d0aa22bf86a

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
 