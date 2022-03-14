function insertData(name, price, comb, tank, trunk, trip){
    $(".name").text(name);
    $(".price").text(betterNumbers(price));
    $(".comb").text(comb);
    $(".tank").text(tank + "l");
    $(".trunk").text(trunk);
    $(".trip").text(trip + " km");
}

$(".button_cancel").on("click", function(){
    mp.trigger("closeVehBuyBrowser");
});

$(".button_select").on("click", function(){
    mp.trigger("buyVehicleHUD");
});

function betterNumbers(val) {
    let spacer = ' ';
    let result = val.toString().trim();
    for(var i = val.toString().trim().length-1; i >= 0; i--) {
        if((val.toString().trim().length - i) % 3 == 0) {
            if(val.toString().slice(0,i) != '')
            result = result.slice(0,i) + spacer + result.slice(i);
        }
    }
    return result;
}