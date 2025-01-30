using UnityEngine;

public class ServerManager : MonoBehaviour
{
    // Temporary solution to get and send data
    // TODO: Connect with web server

    public void SendData(string data)
    {
        Debug.Log(data);
    }

    public string RetrieveData()
    {
        return "Player data retrieved";
    }
}
