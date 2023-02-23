import { executeScript, sendTransaction, shallPass, shallResolve } from "@onflow/flow-js-testing"

// Scripts

export const bb_getBeastIDs = async (account) => {
  const scriptName = "basicbeasts/get_beast_ids"
  const [result, err] = await shallResolve(executeScript({ name: scriptName, args: [account] }))
  return result
}

// Transactions

export const bb_setupAccount = async (signer) => {
  const signers = [signer]
  const txName = "basicbeasts/setup_account"
  const args = []

  await shallPass(sendTransaction({ name: txName, signers: signers, args: args })) 
}

export const bb_createTemplate = async (signer, templateName, templateID, element) => {
  const signers = [signer]
  const txName = "basicbeasts/create_template"
  const args = [templateName, templateID, element]

  await shallPass(sendTransaction({ name: txName, signers: signers, args: args })) 
}

export const bb_mintBeast = async (signer, templateID, recipient) => {
  const signers = [signer]
  const txName = "basicbeasts/mint_beast"
  const args = [templateID, recipient]

  await shallPass(sendTransaction({ name: txName, signers: signers, args: args })) 
}
