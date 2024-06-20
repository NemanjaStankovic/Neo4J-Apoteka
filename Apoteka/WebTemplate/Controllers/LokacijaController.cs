using Neo4jClient;
using Neo4jClient.Cypher;
namespace Apoteka.Models;

[ApiController]
[Route("[controller]")]
public class LokacijaController : ControllerBase
{
    private readonly IGraphClient _client;
    public LokacijaController(IGraphClient client)
    {
        _client = client;
    }
    [HttpGet("sveLokacije")]
    public async Task<IActionResult> sveLokacije()
    {
        string lokacija = "lokacija";
        Dictionary<string, object> queryDict = new Dictionary<string, object>();
        queryDict.Add("lokacija", lokacija);

        var query = new Neo4jClient.Cypher.CypherQuery("match (l:Lokacija) return l order by l.UlicaBr", queryDict, CypherResultMode.Set, "neo4j");
        IEnumerable<Lokacija> lokacije = await ((IRawGraphClient)_client).ExecuteGetCypherResultsAsync<Lokacija>(query);
        List<Lokacija> lokacijaList = lokacije.ToList();
        return Ok(lokacijaList);
    }
    
    [HttpPost("/DodajLokaciju/")]
    public async Task<IActionResult> DodajLokaciju([FromBody]Lokacija lokacija)
    {
        await _client.Cypher.Create("(l:Lokacija $lokacija)").WithParam("lokacija", lokacija).ExecuteWithoutResultsAsync();

        return Ok("Uspesno dodata lokacija!");
    }
    [HttpDelete("/IzbrisiLokaciju/{ulica}")]
    public async Task<IActionResult> IzbrisiLokaciju(string ulica)
    {
        await _client.Cypher.Match("(l:Lokacija)").Where((Lokacija l) => l.UlicaBr == ulica).Delete("l").ExecuteWithoutResultsAsync();

        return Ok(ulica);
    }
    [HttpPut("/UpdateLokacija/{ulica}")]
    public async Task<IActionResult> UpdateLokacija(string ulica, [FromBody]Lokacija lok){
        await _client.Cypher.Match("(l:Lokacija)")
                            .Where((Lokacija l) => l.UlicaBr == ulica)
                            .Set("l = $lok")
                            .WithParam("lok", lok)
                            .ExecuteWithoutResultsAsync();
        return Ok();
    }
}