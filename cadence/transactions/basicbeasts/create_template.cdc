import NonFungibleToken from "../../contracts/core/NonFungibleToken.cdc"
import BasicBeasts from "../../contracts/basicbeasts/BasicBeasts.cdc"

transaction(
    templateName: String,
    templateID: UInt32,
    element: String
) {
    let adminRef: &BasicBeasts.Admin
    prepare(acct: AuthAccount) {
        self.adminRef = acct.borrow<&BasicBeasts.Admin>(from: BasicBeasts.AdminStoragePath)
            ?? panic("Borrow admin failed")
    }

    execute {
        self.adminRef.createBeastTemplate(
            beastTemplateID: templateID, 
            dexNumber: templateID,
            name: templateName,
            description: "For Test",
            image: "Image",
            imageTransparentBg: "Image",
            rarity: "Rarity",
            skin: "Skin",
            starLevel: 0, 
            asexual: true,
            breedableBeastTemplateID: 0,
            maxAdminMintAllowed: 100,
            ultimateSkill: "UltimateSkill",
            basicSkills: ["BasicSkill"],
            elements: [element],
            data: {}
        )
    }
}
