import WonderArenaPawn_BasicBeasts1 from "../contracts/WonderArenaPawn_BasicBeasts1.cdc"

transaction {
    let adminRef: &WonderArenaPawn_BasicBeasts1.Admin
    prepare(acct: AuthAccount) {
        self.adminRef = acct
            .borrow<&WonderArenaPawn_BasicBeasts1.Admin>(from: WonderArenaPawn_BasicBeasts1.AdminStoragePath)
            ?? panic("Could not borrow admin")
    }

    execute {
        let moonTemplate = WonderArenaPawn_BasicBeasts1.PawnTemplate(
            templateName: "Moon",
            type: WonderArenaPawn_BasicBeasts1.PawnType.Electric,
            hp: 70,
            agility: 30,
            speed: 10,
            attack: WonderArenaPawn_BasicBeasts1.PawnAttack(
                value: 30,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToParalysis,
                effectProb: 5
            ),
            defense: 5,
            accuracy: 95, // 95%
            status: WonderArenaPawn_BasicBeasts1.PawnStatus.Normal,
            mana: 0,
            skill: WonderArenaPawn_BasicBeasts1.PawnSkill(
                name: "Mega Volt Crash",
                value: 90,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToParalysis,
                effectProb: 50, // 50%
                manaRequired: 50
            )
        )

        let saberTemplate = WonderArenaPawn_BasicBeasts1.PawnTemplate(
            templateName: "Saber",
            type: WonderArenaPawn_BasicBeasts1.PawnType.Water,
            hp: 70,
            agility: 30,
            speed: 10,
            attack: WonderArenaPawn_BasicBeasts1.PawnAttack(
                value: 30,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.None,
                effectProb: 0
            ),
            defense: 10,
            accuracy: 95,
            status: WonderArenaPawn_BasicBeasts1.PawnStatus.Normal,
            mana: 0,
            skill: WonderArenaPawn_BasicBeasts1.PawnSkill(
                name: "Supersonic Wave",
                value: 50,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToParalysis,
                effectProb: 50,
                manaRequired: 60
            )
        )

        let shenTemplate = WonderArenaPawn_BasicBeasts1.PawnTemplate(
            templateName: "Shen",
            type: WonderArenaPawn_BasicBeasts1.PawnType.Grass,
            hp: 80,
            agility: 30,
            speed: 30,
            attack: WonderArenaPawn_BasicBeasts1.PawnAttack(
                value: 30,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToParalysis,
                effectProb: 5
            ),
            defense: 10,
            accuracy: 95,
            status: WonderArenaPawn_BasicBeasts1.PawnStatus.Normal,
            mana: 0,
            skill: WonderArenaPawn_BasicBeasts1.PawnSkill(
                name: "Tackle",
                value: 60,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToPoison,
                effectProb: 50, // 50%
                manaRequired: 60
            )
        )

        let azazelTemplate = WonderArenaPawn_BasicBeasts1.PawnTemplate(
            templateName: "Azazel",
            type: WonderArenaPawn_BasicBeasts1.PawnType.Fire,
            hp: 60,
            agility: 30,
            speed: 15,
            attack: WonderArenaPawn_BasicBeasts1.PawnAttack(
                value: 45,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToParalysis,
                effectProb: 5
            ),
            defense: 10,
            accuracy: 95,
            status: WonderArenaPawn_BasicBeasts1.PawnStatus.Normal,
            mana: 0,
            skill: WonderArenaPawn_BasicBeasts1.PawnSkill(
                name: "Phantom Force",
                value: 50,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.ToParalysis,
                effectProb: 80, // 50%
                manaRequired: 30
            )
        )

        let teddyTemplate = WonderArenaPawn_BasicBeasts1.PawnTemplate(
            templateName: "Teddy",
            type: WonderArenaPawn_BasicBeasts1.PawnType.Normal,
            hp: 80,
            agility: 20,
            speed: 10,
            attack: WonderArenaPawn_BasicBeasts1.PawnAttack(
                value: 50,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.None,
                effectProb: 0
            ),
            defense: 10,
            accuracy: 95,
            status: WonderArenaPawn_BasicBeasts1.PawnStatus.Normal,
            mana: 0,
            skill: WonderArenaPawn_BasicBeasts1.PawnSkill(
                name: "Hyper Room",
                value: 80,
                effect: WonderArenaPawn_BasicBeasts1.PawnEffect.None,
                effectProb: 0,
                manaRequired: 40
            )
        )

        self.adminRef.addTemplate(moonTemplate)
        self.adminRef.addTemplate(saberTemplate)
        self.adminRef.addTemplate(shenTemplate)
        self.adminRef.addTemplate(azazelTemplate)
        self.adminRef.addTemplate(teddyTemplate)
    }
}