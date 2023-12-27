using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository repo;
        private readonly ContaClienteService serv;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            repo = new ContaClienteRepository();
            serv = new ContaClienteService();
            _cts = new CancellationTokenSource();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            BtnProcessar.IsEnabled = false;

            var contas = repo.GetContaClientes();
            PgsProgresso.Maximum = contas.Count();
            LimparView();

            var byteBankProgress = new ByteBankProgress<string>(str => PgsProgresso.Value++);

            try
            {
                BtnCancelar.IsEnabled = true;

                var inicio = DateTime.Now;
                var resultado = await ConsolidarContas(contas, byteBankProgress, _cts.Token);
                var fim = DateTime.Now;

                AtualizarView(resultado, fim - inicio);                
            }
            catch (OperationCanceledException)
            {
                TxtTempo.Text = "Operação cancelada pelo usuário.";
            }
            finally
            {
                BtnProcessar.IsEnabled = true;
                BtnCancelar.IsEnabled = false;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
            _cts.Cancel();
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> progresso, CancellationToken ct)
        {
            var tasks = contas.Select(c =>
                Task.Factory.StartNew(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    var resultado = serv.ConsolidarMovimentacao(c, ct);
                    progresso.Report(resultado);

                    ct.ThrowIfCancellationRequested();

                    return resultado;
                }, ct)
            );

            return await Task.WhenAll(tasks);
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = null;
            PgsProgresso.Value = 0;
        }

        private void AtualizarView(IEnumerable<string> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}