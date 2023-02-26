import NonFungibleToken from "./core/NonFungibleToken.cdc"
import MetadataViews from "./core/MetadataViews.cdc"
import BasicBeasts from "./basicbeasts/BasicBeasts.cdc"

pub contract WonderArenaPawn_BasicBeasts1 {

    pub let AdminStoragePath: StoragePath
    pub let AdminPublicPath: PublicPath
    pub let AdminPrivatePath: PrivatePath

    pub event ContractInitialized()

    pub event PawnTemplateAdded(templateName: String)
    pub event PawnTemplateRemoved(templateName: String)

    pub enum PawnType: UInt8 {
        pub case Water
        pub case Electric
        pub case Fire
        pub case Normal
        pub case Grass
    }

    pub enum PawnEffect: UInt8 {
        pub case None
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

    pub struct PawnTemplate {
        pub let templateName: String
        pub let type: PawnType
        pub let maxHp: UInt64
        pub let hp: UInt64
        pub let agility: UInt64
        pub let speed: UInt64
        pub let attack: PawnAttack
        pub let defense: UInt64
        pub let accuracy: UInt64
        pub let status: PawnStatus
        pub let mana: UInt64
        pub let skill: PawnSkill

        init(
            templateName: String,
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
            self.templateName = templateName
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
    }

    pub let pawnTemplates: {String: PawnTemplate}

    pub resource Admin {

        pub fun addTemplate(_ template: PawnTemplate) {
            assert(WonderArenaPawn_BasicBeasts1.pawnTemplates[template.templateName] == nil, message: "Template with this name is already exists")
            WonderArenaPawn_BasicBeasts1.pawnTemplates.insert(key: template.templateName, template)
            emit PawnTemplateAdded(templateName: template.templateName)
        }

        pub fun removeTemplate(_ templateName: String) {
            WonderArenaPawn_BasicBeasts1.pawnTemplates.remove(key: templateName)
            emit PawnTemplateRemoved(templateName: templateName)
        }
    }

    pub fun getPawn(beast: &BasicBeasts.NFT{BasicBeasts.Public}): Pawn {
        let beastTemplate = beast.getBeastTemplate()
        if let pawnTemplate = self.pawnTemplates[beastTemplate.name] {
            return Pawn(
                nft: beast,
                type: pawnTemplate.type,
                hp: pawnTemplate.hp,
                agility: pawnTemplate.agility,
                speed: pawnTemplate.speed,
                attack: pawnTemplate.attack,
                defense: pawnTemplate.defense,
                accuracy: pawnTemplate.accuracy,
                status: pawnTemplate.status,
                mana: pawnTemplate.mana,
                skill: pawnTemplate.skill
            )
        }
        panic("Unsupported beast!")
    }

    init() {
        self.pawnTemplates = {}

        self.AdminStoragePath = /storage/WonderArenaPawn_Admin_BasicBeasts1
        self.AdminPublicPath = /public/WonderArenaPawn_Admin_BasicBeasts1
        self.AdminPrivatePath = /private/WonderArenaPawn_Admin_BasicBeasts1

        self.account.save(<- create Admin(), to: self.AdminStoragePath)

        emit ContractInitialized()
    }
}