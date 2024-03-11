// ********************************************************************
// CONFIDENTIAL - DO NOT DISTRIBUTE
// COPYRIGHT 2019-2024 Wacky Potato Games, LLC. All Rights Reserved.
// 
// If you send, receive, or use this file for any purpose other than
// internal use by Wacky Potato Games, it is without permission and an act of theft.
// Report any misuse of this file immediately to contact@wackypotato.com
// Misuse or failure to report misuse will subject you to legal action.
// 
// The intellectual and technical concepts contained herein are
// proprietary and are protected by trade secret and/or copyright law.
// Dissemination or reproduction of this material is forbidden.
// ********************************************************************

using System.Linq;
using UnityEngine;
using static SceneSaver;

public class LoadSlots : MonoBehaviour
{
    const int MAX_LOAD_SLOTS = 6;
    
    public LoadSlotItem[] LoadSlotList;   

    // Start is called before the first frame update
    private void Start()
    {
        RefreshList();
    }

    public void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        ClearList();
        var savedGames = SceneSwapper.Instance.SceneSaver.ReturnSaveGames();
        if(savedGames.Count > 0)
        {
            int selectIndex = LoadSlotList.Length > MAX_LOAD_SLOTS ? 6 : LoadSlotList.Length;
            var lastSaved = savedGames.OrderBy(game => game.Date)
                .OrderByDescending(game => game.Time).Take(selectIndex).ToArray();

            for(int i = 0; i < lastSaved.Length; i++)
            {
                var slot = LoadSlotList[i];
                slot.gameObject.SetActive(true);
                GameObjectData saveSlot = lastSaved[i];
                if(saveSlot != null)
                {
                    LoadSlotList[i].Id.text = saveSlot.Id;
                    var image = SceneSwapper.Instance.SceneSaver.GetImageFromSave(saveSlot);
                    LoadSlotList[i].Image.gameObject.SetActive(true);
                    LoadSlotList[i].Image.texture = image;
                    LoadSlotList[i].SaveName.text = saveSlot.SaveName;
                    LoadSlotList[i].DateTimeStamp.text = $"{saveSlot.Date} { saveSlot.Time}";                    
                }
            }
        }
    }    

    public void LoadMap(string id)
    {
        SceneSwapper.Instance.SceneSaver.LoadGame(id);
    }

    private void ClearList()
    {
        if (LoadSlotList.Length > 0)
        {
            foreach (var item in LoadSlotList)
            {
                item.Id.text = "";
                item.SaveName.text = "";
                item.Image.texture = null;
                item.Image.gameObject.SetActive(false);
                item.DateTimeStamp.text = "";
            }
        }
    }
   
}
