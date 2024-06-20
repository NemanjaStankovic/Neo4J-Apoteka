using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Apoteka.Models;


[ApiController]
[Route("[controller]")]
public class ApotekaController : ControllerBase
{
    private readonly IGraphClient _client;
    public ApotekaController(IGraphClient client)
    {
        _client = client;
    }
    [HttpGet("sveApoteke")]
    public async Task<IActionResult> sveApoteke()
    {
        string apoteka = "apoteka";
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("apoteka", apoteka);

        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) return a order by a.Naziv", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> lokacijaList = apoteke.ToList();
        return Ok(lokacijaList);
    }

    [HttpGet("preuzmiApoteku/{ime}")]
    public async Task<IActionResult> preuzmiApoteku(string ime)
    {
        string apoteka = "apoteka";
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("apoteka", apoteka);

        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) where a.Naziv =~ '"+ ime+"' return a", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> apotekeList = apoteke.ToList();
        var apotekica = apotekeList.FirstOrDefault();
        return Ok(apotekica);
    }

    [HttpPost]
    public async Task<IActionResult> DodajApoteku([FromBody]Apoteka apoteka)
    {
        await _client.Cypher.Create("(a:Apoteka $apoteka)").WithParam("apoteka", apoteka).ExecuteWithoutResultsAsync();

        return Ok("Uspesno dodata apoteka!");
    }

    [HttpPost("/UpdateIma/{nazivApoteke}/{nazivProizvoda}/{kolicina}/{cena}")]
    public async Task<IActionResult> UpdateProizvod(string nazivApoteke, string nazivProizvoda, int kolicina, int cena){

        // Ima ima = new Ima();
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("nazivApoteke", nazivApoteke);

        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) where a.Naziv =~ '"+ nazivApoteke+"' return a", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> apotekeList = apoteke.ToList();
        var apotekica = apotekeList.FirstOrDefault();

        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima) where a.Naziv =~ '" + nazivApoteke +"' and p.Naziv =~ '"+ nazivProizvoda+"' and i.Apoteka = '"+ apotekica.Naziv+"' RETURN i", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Ima> ima1 = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Ima>(query2);
        List<Ima> imalista = ima1.ToList();
        var ima = imalista.FirstOrDefault();

        if(ima!=null)
        {
            if(kolicina!=-1)
                ima.Kolicina = kolicina;
            if(cena!=-1)
                ima.Cena = cena;
                await _client.Cypher.Match("(i:Ima)")
                            .Where((Ima i) => i.Apoteka == apotekica.Naziv && i.Naziv == nazivProizvoda)
                            .Set("i = $ima")
                            .WithParam("ima", ima)
                            .ExecuteWithoutResultsAsync();

        }
        else
        {
            await _client.Cypher.Create("(i:Ima {Apoteka: '"+ nazivApoteke+"', Naziv: '" +nazivProizvoda+ "', Kolicina: '" + kolicina + "', Cena: '"+ cena +"'})").ExecuteWithoutResultsAsync();

        await _client.Cypher.Match("(a:Apoteka), (p:Proizvod), (i:Ima)").Where((Apoteka a, Proizvod p, Ima i) => a.Naziv == nazivApoteke && p.Naziv == nazivProizvoda).Merge("(a)-[:IMA_PROIZVOD]->(p)").ExecuteWithoutResultsAsync();
        //(p)-[b:IMA_KOLICINU]->(i)
        await _client.Cypher.Match("(p:Proizvod), (i:Ima)").Where((Proizvod p, Ima i) => p.Naziv == nazivProizvoda && i.Naziv == nazivProizvoda).Merge("(p)-[:KOLICINU_IMA]->(i)").ExecuteWithoutResultsAsync();
        }
        
        return Ok(ima);
    }

    [HttpDelete("/IzbrisiApoteku/{nazivApoteke}")]
    public async Task<IActionResult> IzbrisiApoteku(string nazivApoteke)
    {
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("nazivApoteke", nazivApoteke);


        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) where a.Naziv =~ '"+ nazivApoteke+"' return a", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> apotekeList = apoteke.ToList();
        var apotekica = apotekeList.FirstOrDefault();

        await _client.Cypher.Match("(a:Apoteka)-[s:SE_NALAZI_U]->(l:Lokacija)").Where((Apoteka a) => a.Naziv == nazivApoteke).Delete("s").ExecuteWithoutResultsAsync();
        await _client.Cypher.Match("(a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)").Where((Apoteka a, Ima i) => a.Naziv == nazivApoteke).Delete("r,a").ExecuteWithoutResultsAsync();
        await _client.Cypher.Match("(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima)").Where((Ima i) => i.Apoteka == apotekica.Naziv).Delete("k,i").ExecuteWithoutResultsAsync();
        return Ok("Uspesno izbrisana Apoteka");
    }
    [HttpPost("{ime}/{ulica}")]
    public async Task<IActionResult> dodajLokacijuApoteke(string ime, string ulica)
    {
        await _client.Cypher.Match("(a:Apoteka), (l:Lokacija)").Where((Apoteka a, Lokacija l) => a.Naziv == ime && l.UlicaBr == ulica).Create("(a)-[r:SE_NALAZI_U]->(l)").ExecuteWithoutResultsAsync();

        return Ok("Uspesno kreirana veza!");
    }

    [HttpPut("UpdateApoteka")]
    public async Task<IActionResult> UpdateApoteka([FromBody]Apoteka apt){
        await _client.Cypher.Match("(a:Apoteka)")
                            .Where((Apoteka a) => a.Naziv == apt.Naziv)
                            .Set("a = $apt")
                            .WithParam("apt", apt)
                            .ExecuteWithoutResultsAsync();
        return Ok();
    }

    [HttpPost("/DodajProizvod/{nazivApoteke}/{nazivProizvoda}/{kolicina}/{cena}")]
    public async Task<IActionResult> dodajProizvod(string nazivApoteke, string nazivProizvoda, int kolicina, int cena)
    {   
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("nazivApoteke", nazivApoteke);


        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) where a.Naziv =~ '"+ nazivApoteke+"' return a", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> apotekeList = apoteke.ToList();
        var apotekica = apotekeList.FirstOrDefault();
        
        var query2 = new Neo4jClient.Cypher.CypherQuery("match (p:Proizvod) where p.Naziv =~ '"+ nazivProizvoda+"' return p", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Proizvod> proizvodi = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Proizvod>(query);
        List<Proizvod> proizvodList = proizvodi.ToList();
        var proizvod = proizvodList.FirstOrDefault();

        Apoteka apotekaObj = new Apoteka();
        apotekaObj.Naziv = apotekica.Naziv;
        apotekaObj.Direktor = apotekica.Direktor;
        apotekaObj.Email = apotekica.Email;
        apotekaObj.BrojTelefona = apotekica.BrojTelefona;

        Proizvod proizvodObj = new Proizvod();
        proizvodObj.Naziv = proizvod.Naziv;
        proizvodObj.Proizvodjac = proizvod.Proizvodjac;
        proizvodObj.Opis = proizvod.Opis;
        
        await _client.Cypher.Create("(i:Ima {Apoteka: '"+ apotekica.Naziv+ "',Naziv: '" +nazivProizvoda+ "', Kolicina: '" + kolicina + "', Cena: '"+ cena +"'})").ExecuteWithoutResultsAsync();

        await _client.Cypher.Match("(a:Apoteka), (p:Proizvod), (i:Ima)").Where((Apoteka a, Proizvod p, Ima i) => a.Naziv == nazivApoteke && p.Naziv == nazivProizvoda).Merge("(a)-[:IMA_PROIZVOD]->(p)").ExecuteWithoutResultsAsync();
        //(p)-[b:IMA_KOLICINU]->(i)
        await _client.Cypher.Match("(p:Proizvod), (i:Ima)").Where((Proizvod p, Ima i) => p.Naziv == nazivProizvoda && i.Naziv == nazivProizvoda).Merge("(p)-[:KOLICINU_IMA]->(i)").ExecuteWithoutResultsAsync();
        return Ok();
        // return Ok("Uspesno kreirana veza!");
    }

    [HttpGet("/VratiKolicinu/{nazivApoteke}/{nazivProizvoda}")]
    public async Task<IActionResult> vratiKolicinu(string nazivApoteke, string nazivProizvoda)
    {
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("nazivApoteke", nazivApoteke);

        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) where a.Naziv =~ '"+ nazivApoteke+"' return a", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> apotekeList = apoteke.ToList();
        var apotekica = apotekeList.FirstOrDefault();

        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima) where a.Naziv =~ '" + nazivApoteke +"' and p.Naziv =~ '"+ nazivProizvoda+"' and i.Apoteka = '"+ apotekica.Naziv+"' RETURN i", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Ima> ima1 = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Ima>(query2);
        List<Ima> imalista = ima1.ToList();
        var ima = imalista.FirstOrDefault();
        // MATCH (a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima) where a.Naziv =~'DM' and p.Naziv =~ 'Aspirin' and i.VezaID = '2' RETURN i
        return Ok(ima.Kolicina);
    }

    [HttpGet("/VratiCenu/{nazivApoteke}/{nazivProizvoda}")]
    public async Task<IActionResult> vratiCenu(string nazivApoteke, string nazivProizvoda)
    {
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("nazivApoteke", nazivApoteke);

        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) where a.Naziv =~ '"+ nazivApoteke+"' return a", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> apotekeList = apoteke.ToList();
        var apotekica = apotekeList.FirstOrDefault();

        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima) where a.Naziv =~ '" + nazivApoteke +"' and p.Naziv =~ '"+ nazivProizvoda+"' and i.Apoteka = '"+ apotekica.Naziv+"' RETURN i", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Ima> ima1 = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Ima>(query2);
        List<Ima> imalista = ima1.ToList();
        var ima = imalista.FirstOrDefault();
        // MATCH (a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima) where a.Naziv =~'DM' and p.Naziv =~ 'Aspirin' and i.VezaID = '2' RETURN i
        return Ok(ima.Cena);
    }

    [HttpGet("/VratiCeoObj/{nazivApoteke}/{nazivProizvoda}")]
    public async Task<IActionResult> vratiCeoObj(string nazivApoteke, string nazivProizvoda)
    {
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("nazivApoteke", nazivApoteke);

        var query = new Neo4jClient.Cypher.CypherQuery("match (a:Apoteka) where a.Naziv =~ '"+ nazivApoteke+"' return a", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Apoteka> apoteke = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Apoteka>(query);
        List<Apoteka> apotekeList = apoteke.ToList();
        var apotekica = apotekeList.FirstOrDefault();

        var query2 = new Neo4jClient.Cypher.CypherQuery("MATCH (a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima) where a.Naziv =~ '" + nazivApoteke +"' and p.Naziv =~ '"+ nazivProizvoda+"' and i.Apoteka = '"+ apotekica.Naziv+"' RETURN i", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Ima> ima1 = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Ima>(query2);
        List<Ima> imalista = ima1.ToList();
        var ima = imalista.FirstOrDefault();
        // MATCH (a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima) where a.Naziv =~'DM' and p.Naziv =~ 'Aspirin' and i.VezaID = '2' RETURN i
        return Ok(ima);
    }


}