let playersPower = 0;

const userCommands = {
    "g": "g {wiadomość}",
    "o": "o {wiadomość}",
    "pm": "pm {nick/id} {wiadomość}",
    "report": "report {nick/id} {powód}",
    "bonus": "bonus",
    "przelew" : "przelew {nick/id}",
    "kierowcy": "kierowcy",
    "reconnect": "reconnect",
    "pmoff": "pmoff {powód}",
    "pmon": "pmon",
    "time": "time"
};

const testerCommands = {
    "klucze": "klucze {id pojazdu} {id/nazwa gracza}",
}

const jModCommands = {
    "a": "a {wiadomość}",
    "fix": "fix {id}",
    "kary": "kary {cały nick}",
    "tpto": "tpto {nick/id}",
    "vtpto": "vtpto {id}",
    "vtphere":"vtphere {id}",
    "dp": "dp {id}",
    "zp": "zp {id}",
    "tphere": "tphere {nick/id}",
    "spec": "spec {nick/id}",
    "stopspec": "stopspec",
    "getowner": "getowner {id}",
    "kick": "kick {nick/id} {powód}",
    "warn": "warn {nick/id} {powód}",
    "tpwp": "tpwp",
    "hash": "hash {hash}",
    "furka": "furka {nazwa}",
    "fdelete": "fdelete",
    "getpos": "getpos {opis}",
    "note": "note {opis}"
}
const modCommands = {
    "coflic": "coflic {nick/id} {powód}",
    "licence": "licence {nick/id} {długość} {powód}",
    "mute": "mute {nick/id} {długość} {powód}",
};

const sModCommands = {
    "ban": "ban {nick/id} {długość} {powód}",
    "tp": "tp {x} {y} {z}",
    "kys": "kys"
}

const jAdminCommands = {
    "setday": "setday",
    "setnight": "setnight"
}

const adminCommands = {

}

const ownerCommands = {
    "speed": "speed {0 - ∞}",
    "power": "power {0 - ∞}",
    "save": "save (zapisuje ustawienia auta)",
    "zds": "zds {prędkość}",


    "inv": "inv",
    "createhouse": "createhouse {typ} {cena}",
    "komis": "komis",
    "additem": "additem {id}"
}


let playerid = 0;
let hidden = true;
let messages = new Array();
let currentIndex = -1;
function sendMessage(id, username, text, type, usertype = "", time, social){
    let scroll = $(".chat_scroll").scrollTop();
    let height = $(".chat_scroll").height();
    let chatHeight = $(".chat_body").height();
    let avatarString = `http://51.38.135.199/avatars/${social}/avatar.png`;
    let moveDown = scroll + height > chatHeight ? true : false;
    var color = "";
    switch(usertype)
    {
        case "owner":
            color = [255, 0, 0, 255]
            break;
        case "admin":
            color = [196, 49, 36, 255]
            break;
        case "jadmin":
            color = [245, 102, 0, 255]
            break;
        case "smod":
            color = [0, 99, 28, 255]
            break;
        case "mod":
            color = [0, 153, 43, 255]
            break;
        case "jmod":
            color = [0, 250, 70, 255]
            break;
        case "tester":
            color = [0, 170, 255, 255]
            break;
        default:
            color = [255, 255, 255, 255]
            break;
    }
    color = rgbToHex(color);
    switch(type)
    {
        case "local":
            showLocalMessage(id, username, text, color, time, avatarString);
            break;
        case "global":
            showGlobalMessage(id, username, text, color, time, avatarString);
            break;
        case "admin":
            showAdminMessage(id, username, text, color, time, avatarString);
            break;
        case "org":
            showOrgMessage(id, username, text, color, time, avatarString);
            break;
        case "private":
            showPrivateMessage(id, username, text, color, time, avatarString);
            break;
        case "info":
            showInfoMessage(text, time, 'img/avatars/logo.png');
            break;
        case "penalty":
            showPenalty(text, time, 'img/avatars/logo.png');
            break;
        case "console":
            showConsoleMessage(text, time, 'img/avatars/logo.png');
            break;
    }
    if(moveDown){
        let element = document.getElementsByClassName("chat_scroll")[0];
        element.scrollTop = element.scrollHeight - element.clientHeight;
    }
}

function showLocalMessage(id, username, text, color, time, avatar){
    let bcolor = "transparent";
    let tcolor = "#fff";
    let mentioned = false;
    if(text.includes("@" + playerid.toString() + " ") || text.endsWith("@" + playerid.toString())){
        mentioned = true;
    }
    
    text = stripHtml(text);
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody" style="background-image: url(${avatar}); border: 1px solid #${color};"></div>
        <div class="chat_messageBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id" style="color: #${color}">[${id}]</p><p class="chat_name">${username}</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-local">Lokalny</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody ${mentioned ? "chat_mentioned" : ""}">
                ${text}
            </div>
        </div>
    </div>
    `);
    
}
function showGlobalMessage(id, username, text, color, time, avatar){
    let bcolor = "transparent";
    let tcolor = "#fff";
    let mentioned = false;
    if(text.includes("@" + playerid.toString() + " ") || text.endsWith("@" + playerid.toString())){
        mentioned = true;
    }
    text = stripHtml(text);
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody" style="background-image: url(${avatar}); border: 1px solid #${color};"></div>
        <div class="chat_messageBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id" style="color: #${color}">[${id}]</p><p class="chat_name">${username}</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-global">Globalny</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody ${mentioned ? "chat_mentioned" : ""}">
                ${text}
            </div>
        </div>
    </div>
    `);
}
function showAdminMessage(id, username, text, color, time, avatar){
    let bcolor = "transparent";
    let tcolor = "#fff";
    let mentioned = false;
    if(text.includes("@" + playerid.toString() + " ") || text.endsWith("@" + playerid.toString())){
        mentioned = true;
    }
    text = stripHtml(text);
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody" style="background-image: url(${avatar}); border: 1px solid #${color};"></div>
        <div class="chat_messageBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id" style="color: #${color}">[${id}]</p><p class="chat_name">${username}</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-admin">Admin</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody ${mentioned ? "chat_mentioned" : ""}">
                ${text}
            </div>
        </div>
    </div>
    `);
}
function showOrgMessage(id, username, text, color, time, avatar){
    let bcolor = "transparent";
    let tcolor = "#fff";
    let mentioned = false;
    if(text.includes("@" + playerid.toString() + " ") || text.endsWith("@" + playerid.toString())){
        mentioned = true;
    }
    text = stripHtml(text);
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody" style="background-image: url(${avatar}); border: 1px solid #${color};"></div>
        <div class="chat_messageBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id" style="color: #${color}">[${id}]</p><p class="chat_name">${username}</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-org">Organizacja</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody ${mentioned ? "chat_mentioned" : ""}">
                ${text}
            </div>
        </div>
    </div>
    `);
}
function showPrivateMessage(id, username, text, color, time, avatar,){
    text = stripHtml(text);
    username = JSON.parse(username);
    let pm;
    switch(username[1]){
        case "to":
            pm = "Do: ";
            break;
        case "from":
            pm = "Od: ";
            break;
    }
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody" style="background-image: url(${avatar}); border: 1px solid rgb(32, 155, 255);"></div>
        <div class="chat_messageBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id"><p class="chat_name">${pm}<p class="chat_id" style="color: #${color}">[${id}]</p>${username[0]}</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-pm">Prywatny</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody">
                ${text}
            </div>
        </div>
    </div>
    `);
}
function showInfoMessage(text, time, avatar){
    text = stripHtml(text);
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody chat_infoBody" style="background-image: url(${avatar})"></div>
        <div class="chat_messageBody chat_infoBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id"><p class="chat_name">Serwer</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-info">Informacja</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody">
                ${text}
            </div>
        </div>
    </div>
    `);
}
function showPenalty(text, time, avatar)
{
    text = stripHtml(text);
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody chat_penaltyBody" style="background-image: url(${avatar})"></div>
        <div class="chat_messageBody chat_penaltyBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id"><p class="chat_name">Serwer</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-penalty">Uwaga</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody">
                ${text}
            </div>
        </div>
    </div>
    `);
}

function showConsoleMessage(text, time, avatar){
    text = stripHtml(text);
    $(".messageBefore").before(`
    <div class="chat_message">
        <div class="chat_avatarBody chat_penaltyBody" style="background-image: url(${avatar})"></div>
        <div class="chat_messageBody">
            <div class="chat_upperBody">
                <div class="chat_userInfo">
                    <p class="chat_id"><p class="chat_name">Serwer</p>
                </div>
                <div class="chat_otherInfo">
                    <div class="chat_type chat_type-console">Console</div><p class="chat_time">${time}</p>
                </div>
            </div>
            <div class="chat_lowerBody">
                ${text}
            </div>
        </div>
    </div>
    `);
}


$("body").on("keyup", function (e) {
    if(e.which == 33){
        $(".chat_scroll").animate({
            scrollTop: $(".chat_scroll").scrollTop() - 100
        }, 200);
    }
    else if(e.which == 34){
        $(".chat_scroll").animate({
            scrollTop: $(".chat_scroll").scrollTop() + 100
        }, 200);
    }
})

document.getElementById("myinput").addEventListener("keyup", function(event){
    if(event.key === "ArrowUp")
    {
        let element = document.getElementById("myinput");
        if(messages.length > 0 && currentIndex < messages.length - 1)
        {
            element.value = messages[currentIndex + 1];
            currentIndex++;
        }
    }
}, true);

document.getElementById("myinput").addEventListener("keyup", function(event){
    if(event.key === "ArrowDown")
    {
        let element = document.getElementById("myinput");
        if(messages.length > 0 && currentIndex > 0)
        {
            element.value = messages[currentIndex - 1];
            currentIndex--;
        }
        else if(currentIndex == 0)
        {
            element.value = "";
            currentIndex--;
        }
    }
}, true);

document.getElementById("myinput").addEventListener("keyup", function(event){
    if(event.key === "Enter")
    {
        let text = document.getElementById("myinput").value;
        document.getElementById("myinput").value = "";
        if(text!="")
            mp.trigger("messageSent", text);
        hideChatInput();
        mp.trigger("setChatEnabled", false);
        messages.unshift(text);
    }
}, true);
 
function showChatInput(){
    let input = document.getElementById("myinput");
    input.style = "display: block";
    input.focus();
    hidden = false;
    currentIndex = -1;
}   

function showChatInputWithSlash(){
    let input = document.getElementById("myinput");
    input.style = "display: block";
    input.focus();
    hidden = false;
    currentIndex = -1;
    input.value = "/";
}   

function hideChatInput(){
    let input = document.getElementById("myinput");
    input.value = "";
    document.activeElement.blur();
    input.style = "display: none";
    hidden = true;
    $(".commandsBlock").empty();
    mp.trigger("setTexting", false);
    let element = document.getElementsByClassName("chat_scroll")[0];
    element.scrollTop = element.scrollHeight - element.clientHeight;
}

function stripHtml(html)
{
   let tmp = document.createElement("DIV");
   tmp.innerHTML = html;
   return tmp.textContent || tmp.innerText || "";
}

function setId(remoteid){
    playerid = remoteid;
}

function setPlayersPower(power){
    playersPower = power;
}

$(".inputBlock").keyup(function(){
    let cB = $(".commandsBlock");
    let phrase = $(this).val();
    cB.empty();
    if(phrase.startsWith('/')){
        phrase = phrase.replace('/', '');
        if(playersPower >= 10){
            for (const command in ownerCommands) {
                if(phrase.length == 0){
                    cB.append(`<div>/${ownerCommands[command]}</div>`);
                }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                    cB.append(`<div>/${ownerCommands[command]}</div>`);
                }
            }
        }
        if(playersPower >= 7){
            for (const command in adminCommands) {
                if(phrase.length == 0){
                    cB.append(`<div>/${adminCommands[command]}</div>`);
                }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                    cB.append(`<div>/${adminCommands[command]}</div>`);
                }
            }
        }
        if(playersPower >= 6){
            for (const command in jAdminCommands) {
                if(phrase.length == 0){
                    cB.append(`<div>/${jAdminCommands[command]}</div>`);
                }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                    cB.append(`<div>/${jAdminCommands[command]}</div>`);
                }
            }
        }
        if(playersPower >= 5){
            for (const command in sModCommands) {
                if(phrase.length == 0){
                    cB.append(`<div>/${sModCommands[command]}</div>`);
                }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                    cB.append(`<div>/${sModCommands[command]}</div>`);
                }
            }
        }
        if(playersPower >= 4){
            for (const command in modCommands) {
                if(phrase.length == 0){
                    cB.append(`<div>/${modCommands[command]}</div>`);
                }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                    cB.append(`<div>/${modCommands[command]}</div>`);
                }
            }
        }
        if(playersPower >= 3){
            for (const command in jModCommands) {
                if(phrase.length == 0){
                    cB.append(`<div>/${jModCommands[command]}</div>`);
                }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                    cB.append(`<div>/${jModCommands[command]}</div>`);
                }
            }
        }
        if(playersPower >= 2){
            for (const command in testerCommands) {
                if(phrase.length == 0){
                    cB.append(`<div>/${testerCommands[command]}</div>`);
                }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                    cB.append(`<div>/${testerCommands[command]}</div>`);
                }
            }
        }
        for (const command in userCommands) {
            if(phrase.length == 0){
                cB.append(`<div>/${userCommands[command]}</div>`);
            }else if(command.startsWith(phrase) || (phrase.includes(' ') && command.startsWith(phrase.split(' ')[0]))){
                cB.append(`<div>/${userCommands[command]}</div>`);
            }
        }
    }
    else{
        cB.empty();
    }
});

var rgbToHex = function (rgb) { 
    var hex = "";
    for(let i = 0; i < 3; i++){
        var number = Number(rgb[i]).toString(16);
        if (number.length < 2) {
            hex += "0" + number;
        }
        else{
            hex += number;
        }
    }
    return hex;
  };

  function checkImage(url) {
      try{
        $(".text").css("background-image", `url(${url})`);
      }
      catch{
          return false;
      }
      return true;
  }