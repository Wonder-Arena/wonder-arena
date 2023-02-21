import * as fcl from "@onflow/fcl";
import elliptic from "elliptic";
import { SHA3 } from "sha3";

const ec = new elliptic.ec("p256");

/**
 * @param {string} addr
 * @returns {Promise<fcl.Account>}
 */
async function getAccount(addr) {
  const account = await fcl.send([fcl.getAccount(addr)]).then(fcl.decode);
  return account;
}

class FlowSigner {
  /** @type {string} */
  address;
  /** @type {string} */
  _privateKeyHex;
  /** @type {number} */
  _accountIndex;
  /** @type {{ [key: string]: string }} */
  _addressMapping;

  /**
   * @param {string} flowAddress
   * @param {string} privateKeyHex
   * @param {number} accountIndex
   * @param {{ [key: string]: string }} addressMapping
   */
  constructor(flowAddress, privateKeyHex, accountIndex, addressMapping) {
    this.address = flowAddress;
    this._privateKeyHex = privateKeyHex;
    this._accountIndex = accountIndex;
    this._addressMapping = addressMapping ?? {};
  }

  /**
   * @param {object} opt
   * @param {string} [opt.address=undefined]
   * @param {number} [opt.accountIndex=undefined]
   * @param {string} [opt.privateKey=undefined]
   */
  buildAuthorization({ address, accountIndex, privateKey } = {}) {
    address = address ?? this.address;
    accountIndex = accountIndex ?? this._accountIndex;
    privateKey = privateKey ?? this._privateKeyHex;
    console.log("Authz Addr:", address, ` - key[${accountIndex}]`);
    /**
     * @param {fcl.Account} account
     * @returns {Promise<fcl.AuthZ>}
     */
    return async (account) => {
      const user = await getAccount(address);
      const key = user.keys[accountIndex];

      return {
        ...account,
        tempId: `${user.address}-${key.index}`,
        addr: fcl.sansPrefix(user.address),
        keyId: Number(key.index),
        signingFunction: (signable) => {
          return {
            addr: fcl.withPrefix(user.address),
            keyId: Number(key.index),
            signature: this._signWithKey(privateKey, signable.message),
          };
        },
      };
    };
  }

  /**
   * @param {string} privateKey
   * @param {string} msg
   */
  _signWithKey(privateKey, msg) {
    const key = ec.keyFromPrivate(Buffer.from(privateKey, "hex"));
    const sig = key.sign(this._hashMsg(msg));
    const n = 32;
    const r = sig.r.toArrayLike(Buffer, "be", n);
    const s = sig.s.toArrayLike(Buffer, "be", n);
    return Buffer.concat([r, s]).toString("hex");
  }

  /**
   * @param {string} msg
   */
  _hashMsg(msg) {
    const sha = new SHA3(256);
    sha.update(Buffer.from(msg, "hex"));
    return sha.digest();
  }

  /**
   * General method of sending transaction
   *
   * @param {string} code
   * @param {fcl.ArgumentFunction} args
   * @param {fcl.AuthorizationFunction} [mainAuthz=undefined]
   * @param {fcl.AuthorizationFunction[]} [extraAuthz=[]]
   */
  async sendTransaction(code, args, mainAuthz = undefined, extraAuthz = []) {
    mainAuthz = mainAuthz ?? this.buildAuthorization();

    const transactionId = await fcl.mutate({
      cadence: replaceImportAddresses(code, this._addressMapping),
      args: args,
      proposer: mainAuthz,
      payer: mainAuthz,
      authorizations:
        extraAuthz.length === 0 ? [mainAuthz] : [mainAuthz, ...extraAuthz],
    });
    console.log("Tx Sent:", transactionId);
    return transactionId;
  }

  /**
   *
   * @param {string} transactionId
   * @param {(txId: string, errorMsg?: string) => void | undefined} onSealed
   * @param {(code: fcl.TransactionStatus) => void | undefined} onStatusUpdated
   * @param {(errorMsg: string) => void | undefined} onErrorOccured
   */
  watchTransaction(transactionId, onSealed, onStatusUpdated, onErrorOccured) {
    fcl.tx(transactionId).subscribe((res) => {
      if (onStatusUpdated) {
        onStatusUpdated(res.status);
      }

      if (res.status === 4) {
        if (res.statusCode !== 0 && onErrorOccured) {
          onErrorOccured(res.errorMessage);
        }
        // on sealed callback
        if (typeof onSealed === "function") {
          onSealed(
            transactionId,
            res.statusCode === 0 ? undefined : res.errorMessage
          );
        }
      }
    });
  }

  /**
   *
   * @param {string} transactionId
   * @param {fcl.TxSubCallback} onSealed
   */
  async onceTransactionSealed(transactionId, onSealed) {
    if (typeof onSealed !== "function") {
      throw new Error("onSealed should be a function");
    }
    return fcl.tx(transactionId).onceSealed(onSealed);
  }

  /**
   *
   * @param {string} code
   * @param {fcl.ArgumentFunction} args
   * @param {any} defaultValue
   * @returns {Promise<any>}
   */
  async executeScript(code, args, defaultValue) {
    try {
      const queryResult = await fcl.query({
        cadence: replaceImportAddresses(code, this._addressMapping),
        args,
      });
      return queryResult ?? defaultValue;
    } catch (e) {
      console.error(e);
      return defaultValue;
    }
  }
}

export default FlowSigner;

/**
 * Returns Cadence template code with replaced import addresses
 *
 * @param {string} code - Cadence template code.
 * @param {((key: string) => string) | { [key: string]: string }} addressMap - name/address map or function to use as lookup table for addresses in import statements.
 * @param {boolean} [byName=true] - lag to indicate whether we shall use names of the contracts.
 * @returns {string}
 */
export function replaceImportAddresses(code, addressMap, byName = true) {
  const REGEXP_IMPORT = /(\s*import\s*)([\w\d]+)(\s+from\s*)([\w\d"-.\\/]+)/g;

  return code.replace(REGEXP_IMPORT, (match, imp, contract, _, address) => {
    const key = byName ? contract : address;
    const newAddress =
      addressMap instanceof Function ? addressMap(key) : addressMap[key];

    // If the address is not inside addressMap we shall not alter import statement
    const validAddress = newAddress || address;
    return `${imp}${contract} from ${validAddress}`;
  });
}