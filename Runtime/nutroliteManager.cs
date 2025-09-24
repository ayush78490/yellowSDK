using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class Web3EventArgs : EventArgs {
  public string Data { get; set; }
}

public class NitroliteManager : MonoBehaviour {
  [DllImport("__Internal")]
  private static extern void InitNitrolite(string channelId, Action onReady);

  [DllImport("__Internal")]
  private static extern void Login(string walletAddress, Action<string> onSuccess, Action<string> onError);

  [DllImport("__Internal")]
  private static extern void GetBalance(string sessionId, Action<string> onBalance);

  [DllImport("__Internal")]
  private static extern void MakeTransaction(string sessionId, string toAddress, string amount, Action<string> onTx, Action<string> onError);

  [DllImport("__Internal")]
  private static extern void CloseSession(string sessionId);

  public string ChannelId { get; private set; } = "your-channel-id"; // Set via inspector or API
  public string SessionId { get; private set; }

  public event EventHandler<Web3EventArgs> OnLoginSuccess;
  public event EventHandler<Web3EventArgs> OnLoginError;
  public event EventHandler<Web3EventArgs> OnBalanceRetrieved;
  public event EventHandler<Web3EventArgs> OnTransactionSent;
  public event EventHandler<Web3EventArgs> OnTransactionError;

  void Start() {
    if (Application.platform == RuntimePlatform.WebGLPlayer) {
      InitNitrolite(ChannelId, OnReady);
    } else {
      Debug.LogWarning("Nitrolite requires WebGL build.");
    }
  }

  private void OnReady() {
    Debug.Log("Nitrolite initialized and connected to ClearNode.");
  }

  public void Login(string walletAddress) {
    Login(walletAddress, OnLoginSuccessInternal, OnLoginErrorInternal);
  }

  private void OnLoginSuccessInternal(string sessionId) {
    SessionId = sessionId;
    OnLoginSuccess?.Invoke(this, new Web3EventArgs { Data = sessionId });
  }

  private void OnLoginErrorInternal(string error) {
    OnLoginError?.Invoke(this, new Web3EventArgs { Data = error });
  }

  public void GetBalance() {
    if (string.IsNullOrEmpty(SessionId)) return;
    GetBalance(SessionId, OnBalanceRetrievedInternal);
  }

  private void OnBalanceRetrievedInternal(string balance) {
    OnBalanceRetrieved?.Invoke(this, new Web3EventArgs { Data = balance });
  }

  public void MakeTransaction(string toAddress, string amount) {
    if (string.IsNullOrEmpty(SessionId)) return;
    MakeTransaction(SessionId, toAddress, amount, OnTransactionSentInternal, OnTransactionErrorInternal);
  }

  private void OnTransactionSentInternal(string txHash) {
    OnTransactionSent?.Invoke(this, new Web3EventArgs { Data = txHash });
  }

  private void OnTransactionErrorInternal(string error) {
    OnTransactionError?.Invoke(this, new Web3EventArgs { Data = error });
  }

  public void Logout() {
    if (!string.IsNullOrEmpty(SessionId)) {
      CloseSession(SessionId);
      SessionId = null;
    }
  }

  void OnApplicationQuit() {
    Logout();
  }
}