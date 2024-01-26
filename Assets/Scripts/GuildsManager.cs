using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using PlayFab.ProfilesModels;
using TMPro;

public class GuildsManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtTitle, txtRefreshCooldown, txtApplied, txtAccepted, txtInvited, txtKicked;
    [SerializeField] TMP_InputField guildIDInput, playerIDInput, txtGuildsDisplay, guildNameInput;
    [SerializeField] GameObject mainMenu, guildUI, guildlessBtns, guildLeaderBtns, guildMemberBtns, createGuildUI;
    [SerializeField] Button refreshListBtn;
    
    public float listRefreshCooldown = 5;
    private float listRefreshTimer;
    private List<GroupWithRoles> guildsList = null;
    private GroupWithRoles playerGuild = null;
    private GroupRole playerRole = null;
    private string titlePlayerID = null;
    private List<EntityMemberRole> guildMembers = null;

    private void OnEnable()
    {
        //Init
        GetPlayerID();
        GetGuild();
        GetGuildsList();
    }

    private void Update()
    {
        //Update cooldown timer
        if (listRefreshTimer > 0)
        {
            refreshListBtn.interactable = false;
            refreshListBtn.image.color = Color.grey;
            //Display cooldown
            txtRefreshCooldown.text = ((int)listRefreshTimer).ToString();
            listRefreshTimer -= Time.deltaTime;
        }
        else
        {
            refreshListBtn.interactable = true;
            refreshListBtn.image.color = Color.white;
            txtRefreshCooldown.text = "";
        }
    }

    public void OnGuildButton()
    {
        mainMenu.SetActive(false);
        guildUI.SetActive(true);
        txtApplied.enabled = false;
        txtAccepted.enabled = false;
        txtInvited.enabled = false;
        txtKicked.enabled = false;

        //If player already has a guild, then show the player's guild information instead of the guild list
        if (playerGuild == null)
        {
            guildlessBtns.SetActive(true);
            guildLeaderBtns.SetActive(false);
            guildMemberBtns.SetActive(false);
            DisplayGuilds(guildsList);
        }
        else
        {
            //If player is an admin, show admin buttons
            if (playerRole.RoleId == "admins")
            {
                guildLeaderBtns.SetActive(true);
            }
            else
            {
                guildLeaderBtns.SetActive(false);
            }
            guildlessBtns.SetActive(false);
            guildMemberBtns.SetActive(true);
            StartCoroutine(DisplayPlayerGuild(playerGuild));
        }
    }

    void OnError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    void GetPlayerID()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetIDSucc, OnError);
    }

    void OnGetIDSucc(GetAccountInfoResult r)
    {
        titlePlayerID = r.AccountInfo.TitleInfo.TitlePlayerAccount.Id;
    }

    public void GetGuild()
    {
        PlayFabGroupsAPI.ListMembership(new ListMembershipRequest(), OnGetGuild, OnError);
    }

    void OnGetGuild(ListMembershipResponse r)
    {
        if (r.Groups.Count > 0)
        {
            playerGuild = r.Groups[0];
            playerRole = playerGuild.Roles[0];
            guildlessBtns.SetActive(false);
            guildMemberBtns.SetActive(true);
            if (playerRole.RoleId == "admins")
            {
                guildLeaderBtns.SetActive(true);
            }
            else
            {
                guildLeaderBtns.SetActive(false);
            }

            StartCoroutine(DisplayPlayerGuild(playerGuild));
        }
        else
        {
            playerGuild = null;
        }
    }

    IEnumerator DisplayPlayerGuild(GroupWithRoles guild)
    {
        GetGuildMembers();

        //Guild information
        txtTitle.text = "Guild";
        txtGuildsDisplay.text = "Name: " + guild.GroupName + "\n";
        txtGuildsDisplay.text += "Guild ID: " + guild.Group.Id + "\n";

        yield return new WaitUntil(() => guildMembers != null);
        //Member information
        guildMembers.ForEach(f =>
        {
            f.Members.ForEach(g =>
            {
                if (g.Key.Id != PlayFabSettings.TitleId)
                {
                    txtGuildsDisplay.text += "\nMember Id: " + g.Key.Id + "\nRole: " + f.RoleName + "\n";
                }
            });
        });
    }

    void GetGuildMembers()
    {
        var guildMembers = new ListGroupMembersRequest
        {
            Group = playerGuild.Group
        };
        PlayFabGroupsAPI.ListGroupMembers(guildMembers, OnGetMembersSucc, OnError);
    }

    void OnGetMembersSucc(ListGroupMembersResponse r)
    {
        guildMembers = r.Members;
    }

    public void OnMembersButton()
    {
        StartCoroutine(DisplayPlayerGuild(playerGuild));
    }

    public void GetGuildsList()
    {
        var guildListCloud = new ExecuteCloudScriptRequest
        {
            FunctionName = "ListTitleMembership",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        };
        PlayFabClientAPI.ExecuteCloudScript(guildListCloud, OnGetGuildsSucc, OnError);
    }
    
    void OnGetGuildsSucc(ExecuteCloudScriptResult r)
    {
        var result = r.FunctionResult as PlayFab.Json.JsonObject;
        //Get List<object>
        var list = result["listTitleMembership"] as PlayFab.Json.JsonObject;
        var newList = list["Groups"];

        var serializer = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
        //Serialize to string first
        string catString = serializer.SerializeObject(newList);
        //Deserialize to List<GroupWithRoles>
        guildsList = serializer.DeserializeObject<List<GroupWithRoles>>(catString);
        
        Debug.Log("Guilds: " + guildsList.Count);
        if (guildsList.Count > 0)
        {
            DisplayGuilds(guildsList);
        }
        else
        {
            txtGuildsDisplay.text = "";
        }

        listRefreshTimer = listRefreshCooldown;
    }

    void DisplayGuilds(List<GroupWithRoles> guildsCache)
    {
        txtTitle.text = "Guilds";
        txtGuildsDisplay.text = "";
        guildsCache.ForEach(f => {
            txtGuildsDisplay.text += f.GroupName + "[" + f.Group.Id + "]\n";
        });
    }

    public void GetGuildInvites()
    {
        PlayFabGroupsAPI.ListMembershipOpportunities(new ListMembershipOpportunitiesRequest(), DisplayInvites, OnError);
    }

    void DisplayInvites(ListMembershipOpportunitiesResponse r)
    {
        txtTitle.text = "Guild Invites";
        txtGuildsDisplay.text = "";

        r.Invitations.ForEach(f =>
        {
            //Loop through guildsList to find invited group name
            for (int i = 0; i < guildsList.Count; i++)
            {
                if (guildsList[i].Group.Id == f.Group.Id)
                {
                    txtGuildsDisplay.text += guildsList[i].GroupName + "[" + f.Group.Id + "]\n" + "Expiring: " + f.Expires + "\n";
                    break;
                }
            }
        });
    }

    public void AcceptInvite()
    {
        if (guildIDInput.text == "")
        {
            return;
        }

        var acceptInvite = new AcceptGroupInvitationRequest
        {
            Group = new PlayFab.GroupsModels.EntityKey
            {
                Id = guildIDInput.text,
                Type = "group"
            }
        };
        PlayFabGroupsAPI.AcceptGroupInvitation(acceptInvite, r => { GetGuild(); }, OnError);
    }

    public void GetGuildApplications()
    {
        var getApplicationsReq = new ListGroupApplicationsRequest
        {
            Group = playerGuild.Group
        };
        PlayFabGroupsAPI.ListGroupApplications(getApplicationsReq, DisplayApplications, OnError);
    }

    void DisplayApplications(ListGroupApplicationsResponse r)
    {
        txtTitle.text = "Guild Applications";
        txtGuildsDisplay.text = "";
        r.Applications.ForEach(f =>
        {
            txtGuildsDisplay.text += "Applicant ID: " + f.Entity.Key.Id + "\n" + "Expiring: " + f.Expires + "\n";
        });
    }

    public void AcceptApplication()
    {
        if (playerIDInput.text == "")
        {
            return;
        }

        var acceptApplication = new AcceptGroupApplicationRequest
        {
            Entity = new PlayFab.GroupsModels.EntityKey
            {
                Id = playerIDInput.text,
                Type = "title_player_account"
            },
            Group = playerGuild.Group
        };
        PlayFabGroupsAPI.AcceptGroupApplication(acceptApplication, r => { txtAccepted.enabled = true; }, OnError);
    }

    public void OnCreateGuildButton()
    {
        createGuildUI.SetActive(true);
    }

    public void OnCreateGuildCancel()
    {
        createGuildUI.SetActive(false);
    }

    public void CreateGuild()
    {
        if (guildNameInput.text == "")
        {
            return;
        }

        var req = new ExecuteCloudScriptRequest
        {
            FunctionName = "CreateTitleGroup",
            FunctionParameter = new
            {
                groupname = guildNameInput.text
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(req, OnCreateGuild, OnError);
    }

    void OnCreateGuild(ExecuteCloudScriptResult r)
    {
        createGuildUI.SetActive(false);
        guildlessBtns.SetActive(false);
        guildLeaderBtns.SetActive(true);
        GetGuild();
    }

    public void ApplyToGuild()
    {
        if (guildIDInput.text == "")
        {
            return;
        }

        var applyToGuildReq = new ApplyToGroupRequest
        {
            Group = new PlayFab.GroupsModels.EntityKey
            {
                Id = guildIDInput.text,
                Type = "group"
            }
        };
        PlayFabGroupsAPI.ApplyToGroup(applyToGuildReq, OnApplicationSucc, OnError);
    }

    void OnApplicationSucc(ApplyToGroupResponse r)
    {
        txtApplied.enabled = true;
    }

    public void InvitePlayer()
    {
        if (playerIDInput.text == "")
        {
            return;
        }

        var inviteReq = new InviteToGroupRequest
        {
            Entity = new PlayFab.GroupsModels.EntityKey
            {
                Id = playerIDInput.text,
                Type = "title_player_account"
            },
            Group = playerGuild.Group
        };
        PlayFabGroupsAPI.InviteToGroup(inviteReq, OnInviteSucc, OnError);
    }

    void OnInviteSucc(InviteToGroupResponse r)
    {
        txtInvited.enabled = true;
    }

    public void KickMember()
    {
        if (playerIDInput.text == "")
        {
            return;
        }

        List<PlayFab.GroupsModels.EntityKey> members = new List<PlayFab.GroupsModels.EntityKey>();

        members.Add(new PlayFab.GroupsModels.EntityKey
        {
            Id = playerIDInput.text,
            Type = "title_player_account"
        });

        var kickReq = new RemoveMembersRequest
        {
            Group = playerGuild.Group,
            Members = members
        };
        PlayFabGroupsAPI.RemoveMembers(kickReq, r => { txtKicked.enabled = true; }, OnError);
    }

    public void LeaveGuild()
    {
        List<PlayFab.GroupsModels.EntityKey> members = new List<PlayFab.GroupsModels.EntityKey>();

        members.Add(new PlayFab.GroupsModels.EntityKey
        {
            Id = titlePlayerID,
            Type = "title_player_account"
        });

        var leaveReq = new ExecuteCloudScriptRequest
        {
            FunctionName = "LeaveGuild",
            FunctionParameter = new
            {
                groupId = playerGuild.Group,
                member = members
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(leaveReq, r => 
        {
            playerGuild = null;
            guildlessBtns.SetActive(true);
            guildMemberBtns.SetActive(false);
            GetGuildsList();
        }, OnError);
    }
}
