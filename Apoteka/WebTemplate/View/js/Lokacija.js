document.getElementById("dodajLokacijuDugme").onclick= ev =>this.dodajLokaciju();

class Lokacija{
    constructor(ulica, broj){
        this.ulica=ulica;
        this.broj=broj;
    }
    crtajOpcije(imeDropDown)
    {
        var se = document.getElementById(imeDropDown);
        var op;
        op = document.createElement("option");
        op.innerHTML = this.ulica+"/"+this.broj;
        op.value = this.ulica;
        se.appendChild(op);
    }
}

function dodajLokaciju() {
    console.log("NESTO0");
    fetch("https://localhost:7080/DodajLokaciju", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            "ulicaBr": document.getElementById("LokUIB").value,
            "brojTelefona": document.getElementById("LokTel").value
        })
    }).then(p => {
        console.log(p);
        if (p.status == 200) {
            alert("uspesno ste dodali lokaciju!");
            location.reload();
        }
        else {
            alert("Doslo je do greske!");
        }
    });
}