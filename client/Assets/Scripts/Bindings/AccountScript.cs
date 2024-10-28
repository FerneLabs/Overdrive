using Dojo;
using Dojo.Starknet;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;


public class AccountScript : MonoBehaviour
{ 
    [SerializeField]
    private PlayerActions playerActions;

	public Account get_account()
	{
        string assetPath = "Assets/Dojo/Runtime/Config/WorldManagerLocalConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<WorldManagerData>(assetPath);

        var privateKey = "0x2bbf4f9fd0bbb2e60b0316c1fe0b76cf7a4d0198bd493ced9b8df2a3a24d68a";
        var address = "0xb3ff441a68610b30fd5e2abbf3a1548eb6ba6f3559f2862bf2dc757e5828ca"; //Address that pays the transactions?

        var provider = new JsonRpcClient(config.rpcUrl);
        var signer = new SigningKey(privateKey);
        var account = new Account(provider, signer, new FieldElement(address));

        return account;
	}

    public async void OnClick()
	{
        Account account = get_account();

        byte[] ba = Encoding.Default.GetBytes("Maur");
        var hexString = BitConverter.ToString(ba);
        hexString = hexString.Replace("-", "");

        await playerActions.create_player(account, new FieldElement(hexString));
	}
}

