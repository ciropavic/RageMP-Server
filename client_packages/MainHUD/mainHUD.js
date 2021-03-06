let dmoney = null;
let dpp = null;
let spacer = ' ';
let time = 0;
let jobInterval;
let earnedMoney = 0;
let earnedEXP = 0;
let earnedPJ = 0;
let earningsPerHour = 0;
let pointsType = "";
let music = null;
let moneyInterval = null;
const defaultHudScale = 1.5;
const defaultChatScale = 1;
const defaultChatWidth = 50;

function setScales(hudScale, chatScale){
    $(".globalContainer").css("width", (defaultChatWidth + (chatScale/4 - 12.5)).toString() + "vh");
    $(".globalContainer").css("font-size", (defaultChatScale + (chatScale/200 - 0.25)).toString() + "vh");
    $("html").css("font-size", (defaultHudScale + (hudScale/100 -0.5)).toString() + "vh");
}
function setAvatar(social){
    let avatarString = `http://51.38.135.199/avatars/${social}/avatar.png`;
    $(".hud_avatar").css("background-image", `url(${avatarString})`);
}
function UpdateInfo(nickname, money, level = "0", exp="0/0"){
    // let currentMoney = $("#money").text().toString();
    if(dmoney != null){
        // currentMoney = currentMoney.slice(0, -1);
        let oldMoney = parseInt(dmoney);
        let newMoney = parseInt(money);
        dmoney = newMoney;
        if(newMoney > oldMoney)
            addMoney(oldMoney, newMoney);
        else if( newMoney < oldMoney)
            takeMoney(oldMoney, newMoney);
    }
    else{
        $(".hud_money").html("$"+ betterNumbers(money));
        dmoney = money;
    }
    $(".hud_lvl").text(level);
    setExp(exp);
    $(".hud_nick").text(nickname);
}

function setExp(e){
    let current = parseInt(e.split('/')[0]);
    let max = parseInt(e.split('/')[1]);
    let gradient = `linear-gradient(90deg, rgba(0,204,153,1) ${parseInt((current/max) * 100)}%, rgb(74, 74, 74) ${parseInt((current/max) * 100) + 5}%, rgb(74, 74, 74) 100%)`;
    $(".hud_avatar_wrap").css("background", gradient);
}

function addMoney(oldMoney, newMoney){
    $(".hud_money").css("color", "#090");
    $(".hud_money").html("+" + betterNumbers(Math.abs(oldMoney - newMoney)) + " $")
    setTimeout(function(){
        $(".hud_money").css("color", "#fff");
        let offset = getOffset(oldMoney, newMoney);
        
        if(moneyInterval != null){
            clearInterval(moneyInterval);
        }
            
        moneyInterval = setInterval(() =>{
            if(oldMoney < newMoney){
                oldMoney += offset;
                $(".hud_money").html(betterNumbers(oldMoney) + " $");
            }
            else if(oldMoney > newMoney){
                $(".hud_money").html(betterNumbers(newMoney) + " $");
                clearInterval(moneyInterval);
                moneyInterval = null;
            }
        }, 2)
    }, 3000);
    
}

function takeMoney(oldMoney, newMoney){
    $(".hud_money").css("color", "#e00");
    $(".hud_money").html("-" + betterNumbers(Math.abs(oldMoney - newMoney)) + " $")
    setTimeout(function(){
        $(".hud_money").css("color", "#fff");
        let offset = getOffset(oldMoney, newMoney);
        if(moneyInterval != null)
            clearInterval(moneyInterval);
        moneyInterval = setInterval(() =>{
            if(oldMoney > newMoney){
                oldMoney -= offset;
                $(".hud_money").html(betterNumbers(oldMoney) + " $");
            }
            else if(oldMoney < newMoney){
                $(".hud_money").html(betterNumbers(newMoney) + " $");
                clearInterval(moneyInterval);
                moneyInterval = null;
            }
        }, 2);
    }, 3000);
    
}

function getOffset(oldMoney, newMoney){
    let offset = Math.abs(oldMoney - newMoney);
    offset = Math.round(offset/10000);
    if(offset == 0)
        return 3;
    else
        return offset * 3;
}

function betterNumbers(val) {
    result = val.toString().trim();
    for(var i = val.toString().trim().length-1; i >= 0; i--) {
        if((val.toString().trim().length - i) % 3 == 0) {
            if(val.toString().slice(0,i) != '')
            result = result.slice(0,i) + spacer + result.slice(i);
        }
    }
    return result;
}

function startJob(workName, type) {
    $(".jobHUD_mainBody").css("display", "flex");
    if(time == 0) {
        jobInterval = setInterval(updateJobTime, 1000);
        $(".jobHUD_name").text(`${workName}`);
        $(".jobHUD_jobType").html(`Zdobyte ${type}:`);
    }
    pointsType = type;
    $(".jobHUD_jp").text("0 "+ pointsType);
}

function stopJob() {
    let hud = $(".jobHUD_mainBody");
    hud.css("display", "none");
    if(!hud.hasClass("jobHUD_hidden"))
        hud.addClass("jobHUD_hidden");
        $(".jobHUD_openText").html("Naci??nij <kbd>B</kbd> aby otworzy?? HUD pracy");
    clearInterval(jobInterval);
    time = 0;
    earnedMoney = 0;
    earnedEXP = 0;
    earningsPerHour = 0;
    earnedPJ = 0;
    $(".jobHUD_time").text('0s');
    $(".jobHUD_earnings").text('0 $');
    $(".jobHUD_exp").text('0 exp');
    $(".jobHUD_average").text('0 $/h');
    $(".jobHUD_jp").text(`0`);
}

function switchJobHUD() {
    let hud = $(".jobHUD_mainBody");
    if(hud.hasClass("jobHUD_hidden")){
        hud.removeClass("jobHUD_hidden");
        $(".jobHUD_openText").html("Naci??nij <kbd>B</kbd> aby zamkn???? HUD pracy");
    }
    else{
        hud.addClass("jobHUD_hidden");
        $(".jobHUD_openText").html("Naci??nij <kbd>B</kbd> aby otworzy?? HUD pracy");
    }
}

function updateJobTime() {
    time++;
    if(time / 3600 >= 1) {
        var h = parseInt(time/3600);
        var m = parseInt((time-h*3600)/60);
        var s = (time-h*3600-m*60);
        $(".jobHUD_time").html(`${h}h ${m}m ${s}s`);
    } else if(time / 60 >= 1) {
        var m = parseInt(time/60);
        var s = parseInt(time-m*60);
        $(".jobHUD_time").html(`${m}m ${s}s`);
    } else {
        $(".jobHUD_time").html(`${time}s`);
    }
    earningsPerHour = earnedMoney/(time/3600);
    $(".jobHUD_average").html(`${betterNumbers(parseInt(earningsPerHour))} $/h`);
}

function updateJobVars(money, pp, pj) {
    earnedMoney += money;
    earnedEXP += pp;
    earnedPJ += pj;
    $(".jobHUD_earnings").text(`${betterNumbers(earnedMoney)} $`);
    $(".jobHUD_exp").text(`${earnedEXP} exp`);
    $(".jobHUD_jp").text(`${earnedPJ} ${pointsType}`);
}

function terminateJob() {
    mp.trigger('terminateJob');

}

function setTime(time){
    $(".hud_time").text(time);
}

function warn(){
    $(".body").css("background-color", "rgba(255,0,0,0.8)")
    setTimeout(function(){
        $(".body").css("background-color", "rgba(255,0,0,0)")
    },1000);
    var audio = new Audio('warn.mp3');
    audio.play();
}

function playmusic(){
    let m = new Audio("pnp.mp3");
    m.play();
}

$("body").on("keyup", function (e) {
    if(!$(".jobHUD_mainBody").hasClass("jobHUD_hidden") && e.which == 35){
        terminateJob();
    }
})