import NonFungibleToken from "./core/NonFungibleToken.cdc"
import MetadataViews from "./core/MetadataViews.cdc"
import BasicBeasts from "./basicbeasts/BasicBeasts.cdc"

pub contract WonderArenaPawn_BasicBeasts1 {

    pub event ContractInitialized()

    pub enum PawnType: UInt8 {
        pub case Water
        pub case Electric
        pub case Fire
        pub case Normal
        pub case Grass
    }

    pub enum PawnEffect: UInt8 {
        pub case ToNormal
        pub case ToParalysis
        pub case ToPoison
        pub case ToSleep
    }

    pub enum PawnStatus: UInt8 {
        pub case Normal
        pub case Paralysis
        pub case Poison
        pub case Sleep
        pub case Defeated
    }

    pub fun effectToStatus(effect: PawnEffect): PawnStatus {
        if effect == PawnEffect.ToNormal {
            return PawnStatus.Normal
        } else if effect == PawnEffect.ToParalysis {
            return PawnStatus.Paralysis
        } else if effect == PawnEffect.ToPoison {
            return PawnStatus.Poison
        } else if effect == PawnEffect.ToSleep {
            return PawnStatus.Sleep
        }
        panic("Invalid effect")
    }

    pub struct PawnAttack {
        pub let value: UInt64
        pub let effect: PawnEffect
        pub let effectProb: UInt64

        init(value: UInt64, effect: PawnEffect, effectProb: UInt64) {
            self.value = value
            self.effect = effect
            self.effectProb = effectProb
        }
    }

    pub struct PawnSkill {
        pub let name: String
        pub let value: UInt64
        pub let effect: PawnEffect
        pub let effectProb: UInt64
        pub let manaRequired: UInt64

        init(
            name: String,
            value: UInt64,
            effect: PawnEffect,
            effectProb: UInt64,
            manaRequired: UInt64
        ) {
            self.name = name
            self.value = value
            self.effect = effect
            self.effectProb = effectProb
            self.manaRequired = manaRequired
        }
    }

    pub struct Pawn {
        pub var nft: &BasicBeasts.NFT{BasicBeasts.Public}

        pub let type: PawnType
        pub let maxHp: UInt64
        pub var hp: UInt64

        pub let agility: UInt64
        pub let speed: UInt64

        pub let attack: PawnAttack
        pub let defense: UInt64
        
        pub let accuracy: UInt64

        pub var status: PawnStatus

        pub var mana: UInt64
        pub let skill: PawnSkill

        init(
            nft: &BasicBeasts.NFT{BasicBeasts.Public},
            type: PawnType,
            hp: UInt64,
            agility: UInt64,
            speed: UInt64,
            attack: PawnAttack,
            defense: UInt64,
            accuracy: UInt64,
            status: PawnStatus,
            mana: UInt64,
            skill: PawnSkill
        ) {
            self.nft = nft
            self.type = type
            self.maxHp = hp
            self.hp = hp
            self.agility = agility
            self.speed = speed
            self.attack = attack
            self.defense = defense
            self.accuracy = accuracy
            self.status = status
            self.mana = mana
            self.skill = skill
        }

        access(account) fun setMana(_ value: UInt64) {
            self.mana = value
        }

        access(account) fun setHp(_ value: UInt64) {
            self.hp = value
        }

        access(account) fun setStatus(_ status: PawnStatus) {
            self.status = status
        }
    }

    access(account) fun getPawn(beast: &BasicBeasts.NFT{BasicBeasts.Public}): Pawn {
        let template = beast.getBeastTemplate()
        let name = template.name
        if name == "Moon" {
            return self.pawnMoon(beast: beast)
        } else if name == "Saber" {
            return self.pawnSaber(beast: beast)
        } else if name == "Shen" {
            return self.pawnShen(beast: beast)
        } else if name == "Azazel" {
            return self.pawnAzazel(beast: beast)
        }
        panic("Unsupported beast!")
    }

    pub fun pawnMoon(beast: &BasicBeasts.NFT{BasicBeasts.Public}): Pawn {
        let skill = PawnSkill(
            name: "Mega Volt Crash",
            value: 100,
            effect: PawnEffect.ToParalysis,
            effectProb: 50, // 50%
            manaRequired: 80
        )

        let attack = PawnAttack(
            value: 30,
            effect: PawnEffect.ToParalysis,
            effectProb: 5
        )

        let pawn = Pawn(
            nft: beast,
            type: PawnType.Electric,
            hp: 100,
            agility: 30,
            speed: 10,
            attack: attack,
            defense: 10,
            accuracy: 95, // 95%
            status: PawnStatus.Normal,
            mana: 0,
            skill: skill
        )

        return pawn
    }

    pub fun pawnSaber(beast: &BasicBeasts.NFT{BasicBeasts.Public}): Pawn {
        let skill = PawnSkill(
            name: "Supersonic Wave",
            value: 100,
            effect: PawnEffect.ToParalysis,
            effectProb: 50, // 50%
            manaRequired: 80
        )

        let attack = PawnAttack(
            value: 30,
            effect: PawnEffect.ToParalysis,
            effectProb: 5
        )

        let pawn = Pawn(
            nft: beast,
            type: PawnType.Water,
            hp: 110,
            agility: 30,
            speed: 10,
            attack: attack,
            defense: 10,
            accuracy: 95, // 95%
            status: PawnStatus.Normal,
            mana: 0,
            skill: skill
        )

        return pawn
    }

    pub fun pawnShen(beast: &BasicBeasts.NFT{BasicBeasts.Public}): Pawn {
        let skill = PawnSkill(
            name: "Tackle",
            value: 100,
            effect: PawnEffect.ToParalysis,
            effectProb: 50, // 50%
            manaRequired: 80
        )

        let attack = PawnAttack(
            value: 35,
            effect: PawnEffect.ToParalysis,
            effectProb: 5
        )

        let pawn = Pawn(
            nft: beast,
            type: PawnType.Grass,
            hp: 140,
            agility: 30,
            speed: 30,
            attack: attack,
            defense: 10,
            accuracy: 95, // 95%
            status: PawnStatus.Normal,
            mana: 0,
            skill: skill
        )

        return pawn
    }

    pub fun pawnAzazel(beast: &BasicBeasts.NFT{BasicBeasts.Public}): Pawn {
        let skill = PawnSkill(
            name: "Phantom Force",
            value: 100,
            effect: PawnEffect.ToParalysis,
            effectProb: 50, // 50%
            manaRequired: 80
        )

        let attack = PawnAttack(
            value: 45,
            effect: PawnEffect.ToParalysis,
            effectProb: 5
        )

        let pawn = Pawn(
            nft: beast,
            type: PawnType.Fire,
            hp: 90,
            agility: 30,
            speed: 15,
            attack: attack,
            defense: 10,
            accuracy: 95, // 95%
            status: PawnStatus.Normal,
            mana: 0,
            skill: skill
        )

        return pawn
    }

    init() {
        emit ContractInitialized()
    }
}