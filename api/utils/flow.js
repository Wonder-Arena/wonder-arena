const fcl = require('@onflow/fcl')

class flowHelper {
  static switchToMainnet = () => {
    fcl
      .config()
      .put("fcl.limit", 9999)
      .put("flow.network", "mainnet")
      .put("accessNode.api", "https://rest-mainnet.onflow.org")
  }

  static switchToTestnet = () => {
    fcl
      .config()
      .put("fcl.limit", 9999)
      .put("flow.network", "testnet")
      .put("accessNode.api", "https://rest-testnet.onflow.org")
      .put("0xWonderArena", "0x2432e062f9f14295")
  }

  static isValidFlowAddress = (address) => {
    if (!address.startsWith("0x") || address.length != 18) {
      return false
    }
  
    const bytes = Buffer.from(address.replace("0x", ""), "hex")
    if (bytes.length != 8) { return false }
    return true
  }
}

module.exports = flowHelper