import BasicBeasts from 0xfa252d0aa22bf86a

pub fun main(address: Address, id: UInt64): &BasicBeasts.NFT{BasicBeasts.Public} {
    let account = getAccount(address)
    let collectionRef = account.getCapability(BasicBeasts.CollectionPublicPath).borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>() 
        ?? panic("Borrow collection failed")
    
    let borrowedBeast = collectionRef.borrowBeast(id: id)!

    return borrowedBeast
}
