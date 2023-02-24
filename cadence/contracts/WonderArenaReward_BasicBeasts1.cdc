import WonderArenaBattleField_BasicBeasts1 from "./WonderArenaBattleField_BasicBeasts1.cdc"
import NonFungibleToken from "./core/NonFungibleToken.cdc"
import BasicBeasts from "./basicbeasts/BasicBeasts.cdc"

pub contract WonderArenaReward_BasicBeasts1 {

    pub let RewardCollectionStoragePath: StoragePath
    pub let RewardCollectionPublicPath: PublicPath
    pub let RewardCollectionPrivatePath: PrivatePath

    pub event ContractInitialized()

    pub event RewardCreated(host: Address, rewardID: UInt64, name: String, scoreThreshold: Int64)
    pub event RewardClaimed(rewardID: UInt64, name: String, claimer: Address, scoreThreshold: Int64, score: Int64, beastID: UInt64)
    pub event RewardDisabled(rewardID: UInt64, name: String)
    pub event RewardEnabled(rewardID: UInt64, name: String)

    pub resource interface IRewardPublic {
        pub let name: String
        pub let description: String
        pub var scoreThreshold: Int64
        pub fun claim(player: &WonderArenaBattleField_BasicBeasts1.Player)
        pub var isEnabled: Bool
        pub let host: Address
        pub var claimedCount: UInt64
        pub fun getAvailableRewards(): UInt64
    }

    pub resource Reward: IRewardPublic {
        pub let name: String
        pub let description: String
        pub let collection: @NonFungibleToken.Collection
        pub var scoreThreshold: Int64
        pub var isEnabled: Bool
        pub let host: Address
        pub var claimedCount: UInt64

        init(
            name: String,
            description: String,
            collection: @NonFungibleToken.Collection,
            scoreThreshold: Int64,
            isEnabled: Bool,
            host: Address
        ) {
            self.name = name
            self.description = description
            self.collection <- collection
            self.scoreThreshold = scoreThreshold
            self.isEnabled = isEnabled
            self.host = host
            self.claimedCount = 0
        }

        pub fun claim(player: &WonderArenaBattleField_BasicBeasts1.Player) {
            assert(self.isEnabled, message: "Not available")
            let beastIDs = self.collection.getIDs()
            assert(beastIDs.length > 0, message: "Reward NFTs not enough")

            let address = player.address
            let score = WonderArenaBattleField_BasicBeasts1.scores[address] ?? 0
            assert(score >= self.scoreThreshold, message: "Score is not enough")

            let collectionRef = getAccount(address)
                .getCapability(BasicBeasts.CollectionPublicPath)
                .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
                ?? panic("Could not borrow player's BasicBeast collection")

            let beast <- self.collection.withdraw(withdrawID: beastIDs[0])
            let beastID = beast.id
            collectionRef.deposit(token: <- beast)
            self.claimedCount = self.claimedCount + 1
            emit RewardClaimed(rewardID: self.uuid, name: self.name, claimer: address, scoreThreshold: self.scoreThreshold, score: score, beastID: beastID)
        }

        pub fun getAvailableRewards(): UInt64 {
            return UInt64(self.collection.getIDs().length)
        }

        pub fun deposit(token: @BasicBeasts.NFT) {
            self.collection.deposit(token: <- token)
        }

        pub fun withdraw(id: UInt64): @NonFungibleToken.NFT {
            return <- self.collection.withdraw(withdrawID: id)
        }

        pub fun setScoreThreshold(_ number: Int64) {
            self.scoreThreshold = number
        }

        pub fun toggleEnabled() {
            self.isEnabled = !self.isEnabled
            if self.isEnabled {
                emit RewardEnabled(rewardID: self.uuid, name: self.name)
            } else {
                emit RewardDisabled(rewardID: self.uuid, name: self.name)
            }
        }

        destroy() {
            pre {
                self.collection.getIDs().length == 0: "collection is not empty, please withdraw all NFTs before delete Reward"
            }

            destroy self.collection
        }
    }

    pub resource interface IRewardCollectionPublic {
        pub fun getAllRewards(): {UInt64: &{IRewardPublic}}
        pub fun borrowPublicReward(id: UInt64): &{IRewardPublic}?
    }

    pub resource RewardCollection: IRewardCollectionPublic {
        pub var rewards: @{UInt64: Reward}

        pub fun createReward(
            name: String,
            description: String,
            collection: @NonFungibleToken.Collection,
            scoreThreshold: Int64,
            isEnabled: Bool
        ): UInt64 {
            let reward <- create Reward(
                name: name,
                description: description,
                collection: <- collection,
                scoreThreshold: scoreThreshold,
                isEnabled: isEnabled,
                host: self.owner!.address
            )

            let rewardID = reward.uuid
            self.rewards[rewardID] <-! reward
            emit RewardCreated(host: self.owner!.address, rewardID: rewardID, name: name, scoreThreshold: scoreThreshold)
            return rewardID
        }

        pub fun getAllRewards(): {UInt64: &{IRewardPublic}} {
            let rewardRefs: {UInt64: &{IRewardPublic}} = {}
            for rewardID in self.rewards.keys {
                let rewardRef = (&self.rewards[rewardID] as &{IRewardPublic}?)!
                rewardRefs.insert(key: rewardID, rewardRef)
            }

            return rewardRefs
        }

        pub fun borrowPublicReward(id: UInt64): &{IRewardPublic}? {
            return &self.rewards[id] as &{IRewardPublic}?
        }

        pub fun borrowReward(id: UInt64): &Reward? {
            return &self.rewards[id] as &Reward?
        }

        init() {
            self.rewards <- {}
        }

        destroy() {
            destroy self.rewards
        }
    }

    pub fun createEmptyRewardCollection(): @RewardCollection {
        return <- create RewardCollection()
    }

    init() {
        self.RewardCollectionStoragePath = /storage/WonderArenaRewardCollection_BasicBeast1
        self.RewardCollectionPublicPath = /public/WonderArenaRewardCollection_BasicBeast1
        self.RewardCollectionPrivatePath = /private/WonderArenaRewardCollection_BasicBeast1

        emit ContractInitialized()
    }
}