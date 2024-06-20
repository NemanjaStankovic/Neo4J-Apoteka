document.getElementById("dodajProizvodDugme").onclick= ev =>this.dodajProizvod();
document.getElementById("dodajLekUApoteku").onclick = ev =>this.dodajLUA();
document.getElementById("proveriKolicinu").onclick = ev =>this.proveriKolicinu();
document.getElementById("proizvodPromeni").onclick= ev =>this.promeniLek();
document.getElementById("proizvodObrisi").onclick= ev =>this.obrisiLek();


class Proizvod{
    constructor(naziv, proizvodjac, kategorija, opis){
        this.naziv=naziv;
        this.proizvodjac=proizvodjac;
        this.kategorija=kategorija;
        this.opis=opis;
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

function dodajProizvod() {
    console.log("NESTO0");
    fetch("https://localhost:7080/DodajProizvod", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            "naziv": document.getElementById("LekNaziv").value,
            "proizvodjac": document.getElementById("LekPro").value,
            "kategorija": document.getElementById("LekKat").value,
            "opis": document.getElementById("LekOpis").value
        })
    }).then(p => {
        console.log(p);
        if (p.status == 200) {
            alert("uspesno ste dodali proizvod!");
            location.reload();
        }
        else {
            alert("Doslo je do greske!");
        }
    });
}

function dodajLUA(){
    var kolicina = -1;
    var cena = -1;
    console.log(document.getElementById("lekKol").value+"/"+document.getElementById("lekCena").value);
    if(document.getElementById("lekKol").value!='')
        kolicina = document.getElementById("lekKol").value;
    if(document.getElementById("lekCena").value!='')
        cena = document.getElementById("lekCena").value;
    console.log(kolicina+"/"+cena);
    if(document.getElementById("lekKol").value==0)
    {
        fetch("https://localhost:7080/IzbrisiProizvod/"+document.getElementById("proizvodiDD").value+"/"+document.getElementById("apotekaDD").value, {method: "DELETE"}).then(p=>
            {if(p.status ==200){alert("Lek uspesno uklonjen iz apoteke!");location.reload();}else{alert("Doslo je do greske");}}
        )
    }
    else{
        fetch("https://localhost:7080/UpdateIma/"+document.getElementById("apotekaDD").value+"/"+document.getElementById("proizvodiDD").value+"/"+kolicina+"/"+cena, {
        method: "POST"
        }).then(p => {
            console.log(p);
            if (p.status == 200) {
                alert("uspesno ste promenili stanje!");
                location.reload();
            }
            else {
                if(p.status == 204)
                    alert("Uspesno dodat!");
                else
                    alert("Doslo je do greske!");
                location.reload();
            }
        });
    }
    
}

function proveriKolicinu(){
    fetch("https://localhost:7080/VratiCeoObj/"+document.getElementById("apotekaDD").value+"/"+document.getElementById("proizvodiDD").value)
                .then(p => {
                    if(p.status==204){document.getElementById("kolicinaPrikaz").innerHTML="Nema ga na stanju";}
                    else
                    {
                       p.json().then(proizvod => {
                            console.log(proizvod);
                            document.getElementById("kolicinaPrikaz").innerHTML="Kolicina: "+proizvod.kolicina+", Cena: "+proizvod.cena;
                        }); 
                    }
                    })
}

function promeniLek(){
    fetch("https://localhost:7080/Proizvod/preuzmiProizvod/"+ document.getElementById("proizvodiUD").value,{
        method: "GET"})
        .then(p=> { 
        p.json().then(apoteka => {
                console.log(apoteka);
                document.getElementById("LekNaziv").value = apoteka.naziv;
                document.getElementById("LekPro").value = apoteka.proizvodjac;
                document.getElementById("LekKat").value = apoteka.kategorija;
                document.getElementById("LekOpis").value = apoteka.opis;
            })})
        var roditelj = document.getElementById("promenaLekaa");
        var dugme = document.createElement("input");
        dugme.type="button";
        dugme.value="Izmeni";
        dugme.id = "izmeniLekDugme";
        dugme.onclick = ev => this.izmeniLek();

        var dugmeZaSakrivanje = document.getElementById("dodajProizvodDugme");
        dugmeZaSakrivanje.style.visibility='hidden';
        
        var imeLeka = document.getElementById("LekNaziv");
        imeLeka.disabled=true;

        var dete = document.getElementById("izmeniLekDugme");
        if(dete!=null)
            roditelj.removeChild(dete);
        roditelj.appendChild(dugme);
}
function izmeniLek()
{
    fetch("https://localhost:7080/Proizvod/UpdateProizvod/",{
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            "naziv": document.getElementById("LekNaziv").value,
            "proizvodjac": document.getElementById("LekPro").value,
            "kategorija": document.getElementById("LekKat").value,
            "opis": document.getElementById("LekOpis").value
        })
    }).then(p => {
        console.log(p);
        if (p.status == 200) {
            alert("uspesno ste izmenili proizvod!");
            location.reload();
        }
        else {
            alert("Doslo je do greske!");
        }
    });
}
function obrisiLek(){
    fetch("https://localhost:7080/IzbrisiProizvodSkroz/" + document.getElementById("proizvodiUD").value, {
        method: 'DELETE',
    })
        .then(p => {
            if (p.status == 200) {
                alert("uspesno ste obrisali lek!");
                location.reload();
            }
            else {
                alert("Doslo je do greske!");
            }
        });
}