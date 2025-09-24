mergeInto(LibraryManager.library, {
  // Initialize NitroliteClient and connect to ClearNode
  InitNitrolite: function(channelIdPtr, onReadyPtr) {
    var channelId = UTF8ToString(channelIdPtr);
    var sdk = require('@erc7824/nitrolite'); // Assumes bundled or global
    var client = new sdk.NitroliteClient(channelId, { wsUrl: 'wss://clearnet.yellow.com/ws' });
    client.connect().then(() => {
      if (onReadyPtr) {
        dynCall_v.call(onReadyPtr); // Callback to C#
      }
    }).catch(console.error);
  },

  // Login: Connect wallet (e.g., MetaMask) and create/join session
  Login: function(walletAddressPtr, onSuccessPtr, onErrorPtr) {
    var address = UTF8ToString(walletAddressPtr);
    if (typeof window.ethereum !== 'undefined') {
      window.ethereum.request({ method: 'eth_requestAccounts' }).then(accounts => {
        if (accounts[0].toLowerCase() === address.toLowerCase()) {
          // Create application session
          client.createSession({ appId: 'unity-app' }).then(session => {
            if (onSuccessPtr) dynCall_v.call(onSuccessPtr, [session.id]); // Pass session ID
          }).catch(err => {
            if (onErrorPtr) dynCall_v.call(onErrorPtr, [stringToNewUTF8(err.message)]);
          });
        }
      });
    }
  },

  // Get balance: Query channel balance
  GetBalance: function(sessionIdPtr, onBalancePtr) {
    var sessionId = UTF8ToString(sessionIdPtr);
    client.getBalance(sessionId).then(balance => {
      var balanceStr = balance.toString(); // e.g., wei
      var buffer = stringToNewUTF8(balanceStr);
      if (onBalancePtr) dynCall_v.call(onBalancePtr, [buffer]);
    }).catch(console.error);
  },

  // Make transaction: Off-chain transfer
  MakeTransaction: function(sessionIdPtr, toAddressPtr, amountPtr, onTxPtr, onErrorPtr) {
    var sessionId = UTF8ToString(sessionIdPtr);
    var to = UTF8ToString(toAddressPtr);
    var amount = UTF8ToString(amountPtr);
    client.transfer(sessionId, { to: to, amount: amount }).then(txHash => {
      if (onTxPtr) dynCall_v.call(onTxPtr, [stringToNewUTF8(txHash)]);
    }).catch(err => {
      if (onErrorPtr) dynCall_v.call(onErrorPtr, [stringToNewUTF8(err.message)]);
    });
  },

  // Close session
  CloseSession: function(sessionIdPtr) {
    var sessionId = UTF8ToString(sessionIdPtr);
    client.closeSession(sessionId);
  }
});