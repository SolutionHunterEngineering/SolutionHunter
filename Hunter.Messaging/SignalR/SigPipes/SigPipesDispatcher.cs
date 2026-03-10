using System.IO.Pipes;
using System.Text.Json;
using Messaging.Abstractions;

namespace SigPipes
{
    public static class SigPipesDispatcher
    {
        private const string PipeName = "HunterDispatch";

        public static async Task<object?> SendPipesAsync(TransportDTO dto, CancellationToken token = default)
        {
            try
            {
                string serialized = JsonSerializer.Serialize(dto);

                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await client.ConnectAsync(token);

                using var writer = new StreamWriter(client) { AutoFlush = true };
                using var reader = new StreamReader(client);

                await writer.WriteLineAsync(serialized);

                if (!dto.IsFireAndForget)
                {
                    string? responseLine = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(responseLine))
                        return null;

                    return JsonSerializer.Deserialize<SigPipeResponse>(responseLine);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[PipeDispatcher] SendPipesAsync failed: {ex.Message}");
                return null;
            }
        }
    }
}
