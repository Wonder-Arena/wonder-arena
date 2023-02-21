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
  }
}

module.exports = flowHelper