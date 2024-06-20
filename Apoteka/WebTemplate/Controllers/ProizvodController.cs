using Neo4jClient;
using Neo4jClient.Cypher;
namespace Apoteka.Models;


[ApiController]
[Route("[controller]")]

public class ProizvodController : ControllerBase
{
    private readonly IGraphClient _client;
    public ProizvodController(IGraphClient client)
    {
        _client = client;
    }

    [HttpGet("sviProizvodi")]
    public async Task<IActionResult> sviProizvodi()
    {
        string proizvod = "proizvod";
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("proizvod", proizvod);

        var query = new Neo4jClient.Cypher.CypherQuery("match (p:Proizvod) return p order by p.Naziv", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Proizvod> proizvodi = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Proizvod>(query);
        List<Proizvod> lokacijaList = proizvodi.ToList();
        return Ok(lokacijaList);
    }

    [HttpGet("preuzmiProizvod/{ime}")]
    public async Task<IActionResult> preuzmiApoteku(string ime)
    {
        string proizvod = "proizvod";
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("proizvod", proizvod);

        var query = new Neo4jClient.Cypher.CypherQuery("match (p:Proizvod) where p.Naziv =~ '"+ ime+"' return p", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Proizvod> proizvodi = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Proizvod>(query);
        List<Proizvod> proizvodiList = proizvodi.ToList();
        var proizvod123 = proizvodiList.FirstOrDefault();
        return Ok(proizvod123);
    }

    [HttpPost("/DodajProizvod/")]
    public async Task<IActionResult> DodajProizvod([FromBody]Proizvod proizvod)
    {
        await _client.Cypher.Create("(a:Proizvod $proizvod)").WithParam("proizvod", proizvod).ExecuteWithoutResultsAsync();

        return Ok("Uspesno dodat proizvod!");
    }
    [HttpDelete("/IzbrisiProizvod/{ime}/{apo}")]
    public async Task<IActionResult> IzbrisiProizvod(string ime, string apo)
    {
        await _client.Cypher.Match("(a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima)").Where((Proizvod p, Apoteka a) => p.Naziv == ime && a.Naziv==apo).Delete("r,k,i").ExecuteWithoutResultsAsync();

        return Ok(ime);
    }
    [HttpDelete("/IzbrisiProizvodSkroz/{proizovd}")]
    public async Task<IActionResult> IzbrisiApoteku(string proizovd)
    {
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("nazivApoteke", proizovd);


        var query = new Neo4jClient.Cypher.CypherQuery("match (p:Proizvod) where p.Naziv =~ '"+ proizovd+"' return p", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Proizvod> proizvodi = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Proizvod>(query);
        List<Proizvod> proizvodiList = proizvodi.ToList();
        var proPom = proizvodiList.FirstOrDefault();

        await _client.Cypher.Match("(p:Proizvod)-[k:KOLICINU_IMA]->(i:Ima)").Where((Proizvod p) => p.Naziv == proizovd).Delete("k, i").ExecuteWithoutResultsAsync();
        await _client.Cypher.Match("(a:Apoteka)-[r:IMA_PROIZVOD]->(p:Proizvod)").Where((Apoteka a, Proizvod p) => p.Naziv == proizovd).Delete("r,p").ExecuteWithoutResultsAsync();
        return Ok("Uspesno izbrisana Apoteka");
    }
    [HttpPut("UpdateProizvod")]
    public async Task<IActionResult> UpdateProizvod([FromBody]Proizvod pro){
        await _client.Cypher.Match("(p:Proizvod)")
                            .Where((Proizvod p) => p.Naziv == pro.Naziv)
                            .Set("p = $pro")
                            .WithParam("pro", pro)
                            .ExecuteWithoutResultsAsync();
        return Ok();
    }
}