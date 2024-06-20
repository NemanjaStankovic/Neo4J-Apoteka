document.getElementById("dodajApotekuDugme").onclick= ev =>this.dodajApoteku();
document.getElementById("apotekaPromeni").onclick= ev =>this.promeniApoteku();
document.getElementById("apotekaObrisi").onclick= ev =>this.obrisiApoteku();

class Apoteka{
    constructor(naziv, email, direktor, brTelefona){
        this.naziv = naziv;
        this.email = email;
        this.direktor = direktor;
        this.brTelefona = brTelefona;
    }
    crtajOpcije(imeDropDown)
    {
        var se = document.getElementById(imeDropDown);
        var op;
        op = document.createElement("option");
        op.innerHTML = this.naziv;
        op.value = this.naziv;
        se.appendChild(op);
    }
}

function dodajApoteku() {
    fetch("https://localhost:7080/Apoteka", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            "naziv": document.getElementById("ApoNaziv").value,
            "email": document.getElementById("ApoEmail").value,
            "direktor": document.getElementById("ApoDirektor").value,
            "brTelefona": document.getElementById("ApoBrTel").value
        })
    }).then(p => {
        if (p.status == 200) {
            fetch("https://localhost:7080/Apoteka/"+document.getElementById("ApoNaziv").value+"/"+document.getElementById("myDropdown").value, {
                method: "POST"}).then(p=>{if(p.status==200)alert("Uspesno dodata veza!");})
            alert("uspesno ste dodali apoteku!");
            location.reload();
        }
        else {
            alert("Doslo je do greske!");
        }
    });
}

function promeniApoteku(){
    fetch("https://localhost:7080/Apoteka/preuzmiApoteku/"+ document.getElementById("apotekaUD").value,{
        method: "GET"})
        .then(p=> { 
        p.json().then(apoteka => {
                console.log(apoteka);
                document.getElementById("ApoNaziv").value = apoteka.naziv;
                document.getElementById("ApoEmail").value = apoteka.email;
                document.getElementById("ApoDirektor").value = apoteka.direktor;
                document.getElementById("ApoBrTel").value = apoteka.brojTelefona;
            })})
        var roditelj = document.getElementById("promenaApotekee");
        var dugme = document.createElement("input");
        dugme.type="button";
        dugme.value="Izmeni";
        dugme.id = "izmeniApotekuDugme";
        dugme.onclick = ev => this.izmeniApoteku();
        
        var dugmeZaSakrivanje = document.getElementById("dodajApotekuDugme");
        dugmeZaSakrivanje.style.visibility='hidden';

        var imeApoteke = document.getElementById("ApoNaziv");
        imeApoteke.disabled=true;

        var dete = document.getElementById("izmeniApotekuDugme");
        if(dete!=null)
            roditelj.removeChild(dete);
        roditelj.appendChild(dugme);
}
function izmeniApoteku()
{
    fetch("https://localhost:7080/Apoteka/UpdateApoteka",{
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            "naziv": document.getElementById("ApoNaziv").value,
            "email": document.getElementById("ApoEmail").value,
            "direktor": document.getElementById("ApoDirektor").value,
            "brojTelefona": document.getElementById("ApoBrTel").value
        })
    }).then(p => {
        console.log(p);
        if (p.status == 200) {
            alert("uspesno ste izmenili apoteku!");
            location.reload();
        }
        else {
            alert("Doslo je do greske!");
        }
    });
}
function obrisiApoteku(){
    fetch("https://localhost:7080/IzbrisiApoteku/" + document.getElementById("apotekaUD").value, {
        method: "DELETE",
    }).then(p => {
            if (p.status == 200) {
                alert("uspesno ste obrisali apoteku!");
                location.reload();
            }
            else {
                alert("Doslo je do greske!");
            }
        });
}