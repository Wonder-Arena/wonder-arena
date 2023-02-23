import { executeScript, sendTransaction, shallPass, shallResolve } from "@onflow/flow-js-testing"

// scripts

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