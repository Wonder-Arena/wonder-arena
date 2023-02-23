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

export const getScores = async (addresses) => {
  const scriptName = "get_scores"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [addresses] }))
  return result
}

export const getAttackerChallenges = async (attacker, defender) => {
  const scriptName = "get_attacker_challenges"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [attacker, defender] }))
  return result
}

export const getRules = async () => {
  const scriptName = "get_rules"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [] }))
  return result
}

export const getPawns = async (alice, beastIDs) => {
  const scriptName = "get_pawns"
  return await executeScript({ name: scriptName, args: [alice, beastIDs] })
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
