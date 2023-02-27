import { executeScript, sendTransaction, shallPass, shallResolve } from "@onflow/flow-js-testing"

// scripts

export const getDefenderGroups = async (account) => {
  const scriptName = "get_defender_groups"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [account] }))
  return result
}

export const getPlayers = async () => {
  const scriptName = "get_players"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [] }))
  return result
}

export const getPlayersWithScore = async () => {
  const scriptName = "get_players_with_score"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [] }))
  return result
}

export const getAttackRecords = async (attacker, defender) => {
  const scriptName = "get_attack_records"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [attacker, defender] }))
  return result
}

export const getRules = async () => {
  const scriptName = "get_rules"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [] }))
  return result
}

export const getPawns = async (account, beastIDs) => {
  const scriptName = "get_pawns"
  return await executeScript({ name: scriptName, args: [account, beastIDs] })
}

export const getRewards = async (account) => {
  const scriptName = "get_rewards"
  return await executeScript({ name: scriptName, args: [account] })
}

export const getLinkedChildren = async (account) => {
  const scriptName = "get_linked_children"
  return await executeScript({ name: scriptName, args: [account] })
}

// transactions

export const createPlayer = async (signer, name) => {
  const signers = [signer]
  const txName = "create_player"
  const args = [name]

  return await sendTransaction({ name: txName, signers: signers, args: args }) 
}

export const register = async (signer, address) => {
  const signers = [signer]
  const txName = "register"
  const args = [address]

  return await sendTransaction({ name: txName, signers: signers, args: args }) 
}

export const addLink = async (signer, parent, child) => {
  const signers = [signer]
  const txName = "add_link"
  const args = [parent, child]

  return await sendTransaction({ name: txName, signers: signers, args: args }) 
}

export const addDefenderGroup = async (signer, name, beastIDs) => {
  const signers = [signer]
  const txName = "add_defender_group"
  const args = [name, beastIDs]

  return await sendTransaction({ name: txName, signers: signers, args: args }) 
}

export const removeDefenderGroup = async (signer, name) => {
  const signers = [signer]
  const txName = "remove_defender_group"
  const args = [name]

  return await sendTransaction({ name: txName, signers: signers, args: args }) 
}

export const updateGroupSize = async (signer, size) => {
  const signers = [signer]
  const txName = "update_group_size"
  const args = [size]

  return await sendTransaction({ name: txName, signers: signers, args: args }) 
}

export const updateMaxGroupNumber = async (signer, number) => {
  const signers = [signer]
  const txName = "update_max_group_number"
  const args = [number]

  return await sendTransaction({ name: txName, signers: signers, args: args }) 
}

export const fight = async (signer, attackerAddress, attackerIDs, defenderAddress) => {
  const signers = [signer]
  const txName = "fight"
  const args = [attackerAddress, attackerIDs, defenderAddress]

  return await sendTransaction({ name: txName, signers: signers, args: args, limit: 9999 }) 
}

export const setupRewardCollection = async (signer) => {
  const signers = [signer]
  const txName = "setup_reward_collection"
  const args = []

  return await sendTransaction({ name: txName, signers: signers, args: args, limit: 9999 }) 
}

export const createReward = async (signer, name, description, beastIDs, scoreThreshold, isEnabled) => {
  const signers = [signer]
  const txName = "create_reward"
  const args = [name, description, beastIDs, scoreThreshold, isEnabled]

  return await sendTransaction({ name: txName, signers: signers, args: args, limit: 9999 }) 
}

export const claimReward = async (signer, host, rewardID) => {
  const signers = [signer]
  const txName = "claim_reward"
  const args = [host, rewardID]

  return await sendTransaction({ name: txName, signers: signers, args: args, limit: 9999 }) 
}

export const setPawnTemplates = async (signer) => {
  const signers = [signer]
  const txName = "set_pawn_templates"
  const args = []

  return await sendTransaction({ name: txName, signers: signers, args: args, limit: 9999 }) 
}
