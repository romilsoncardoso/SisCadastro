using System;
using System.Data;
using System.Windows.Forms;

//Referência a Dll para utilização do MySql
using MySql.Data.MySqlClient;

namespace SisCadastro
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();

            //Bloqueia o dataGridView para não criar colunas automáticamente, 
            //mostrando assim somente as colunas criadas manualmente
            dataGridView1.AutoGenerateColumns = false;
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {

        }

        #region Funções com Acesso ao Banco de Dados

        //Variável global com a string de conexão com o banco de dados MySql
        private string sConnection = "Server=Localhost;Port=3309;Database=SisCadastro;UserId=root;Password='';";

        /// <summary>
        /// Instância um novo objeto MySqlConnection para efetuar a conexão com o banco
        /// </summary>
        /// <returns>Retorna o objeto MySqlConnection criado</returns>
        private MySqlConnection CriaConexaoBanco()
        {
            //Cria uma nova intância do objeto MySqlConnection
            //passando como parâmetro a string de conexão com o banco
            return new MySqlConnection(sConnection);
        }

        /// <summary>
        /// Função para executar manipulações no banco de dados (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="sSql">String com o comando SQL a ser executado no banco de dados</param>
        /// <returns>Retorna [True] para uma manipulação completa sem erro e [False] caso de algum erro na mesma</returns>
        private bool ExecutaManipulacao(string sSql)
        {

            //Criar a conexão
            MySqlConnection mySqlConnection = CriaConexaoBanco();

            try
            {
                //Abre a conexão com o banco de dados
                mySqlConnection.Open();

                //Criar o comando que vai levar as informações para o banco
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();

                //Colocando as coisas dentro do comando (dentro da caixa que vai trafegar na conexão)
                mySqlCommand.CommandText    = sSql;
                mySqlCommand.CommandType    = CommandType.Text;
                mySqlCommand.CommandTimeout = 7200; //Em Segundos

                //Executar o comando, ou seja, mandar o comando ir até o banco de dados
                mySqlCommand.ExecuteScalar();

                //Se não houve nenhum erro na manipulação retorna [True]
                return true;
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro e retorna [False]
                MessageBox.Show("Erro ao executar manipulação no banco de dados!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                //Fecha a conexão
                mySqlConnection.Close();
            }
        }

        /// <summary>
        /// Função para executar uma consulta no banco de dados (SELECT)
        /// </summary>
        /// <param name="sSql">String com o comando SQL a ser executado no banco de dados</param>
        /// <returns>Retorna um dataTable com os dados encontrados no banco</returns>
        private DataTable ExecutaConsulta(string sSql)
        {
            //Criar a conexão
            MySqlConnection mySqlConnection = CriaConexaoBanco();

            try
            {
                //Abre a conexão com o banco de dados
                mySqlConnection.Open();

                //Criar o comando que vai levar as informações para o banco
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();

                //Colocando as coisas dentro do comando (dentro da caixa que vai trafegar na conexão)
                mySqlCommand.CommandText    = sSql;
                mySqlCommand.CommandType    = CommandType.Text;
                mySqlCommand.CommandTimeout = 7200; //Em Segundos

                //Criando um adaptador
                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);

                //DataTable = Tabela de Dados vazia onde vou colocar os dados que vem do banco
                DataTable dataTable = new DataTable();

                //Mandar o comando ir até o banco buscar os dados e o adaptador preencher o dataTable
                mySqlDataAdapter.Fill(dataTable);

                //Retorna o dataTable com os dados vindos do banco de dados
                return dataTable;
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro e retorna null
                MessageBox.Show("Erro ao executar consulta no banco de dados!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                //Fecha a conexão
                mySqlConnection.Close();
            }
        }

        #endregion

        private void FrmPrincipal_KeyDown(object sender, KeyEventArgs e)
        {
            //Para esses eventos abaixo funcionarem a propriedade KeyPreview deve estar setada como True;
            switch (e.KeyCode)
            {
                //Fecha o Sistema com o Esc
                case Keys.Escape:
                    Application.Exit();
                    break;
                //Usa a tecla Enter como o Tab para pular os campos
                case Keys.Enter:
                    this.SelectNextControl(this.ActiveControl, !e.Shift, true, true, true);
                    break;
                //Pesquisa os registros chamando o click do botão pesquisa  pela tecla F3
                case Keys.F3:
                    btnPesquisar_Click(null, null);
                    break;
            }
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            //Chama a função para carregar o grid com os dados do banco de dados
            CarregaGridCliente();

            //Seleciona a tab de lista no tabControl1
            tabControl1.SelectTab(tabPage1);
        }

        private void btnNovo_Click(object sender, EventArgs e)
        {
            //Seleciona a tab de cadastro no tabControl1
            tabControl1.SelectTab(tabPage2);

            //Limpa os campos da tab de cadastro
            LimparCampos(tabPage2);

            //Coloca o foco no botão nome
            txtNome.Focus();
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            //Verifica se o campo de Nome está preenchido
            if (txtNome.Text == "")
            {
                MessageBox.Show("O campo Nome deve ser preechido!", "Atençaõ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            //Verifica se o campo de Cpf está preenchido
            if (txtCpf.Text == "")
            {
                MessageBox.Show("O campo Cpf deve ser preenchido!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCpf.Focus();
                return;
            }

            //Verifica se já tem algum registro com esse Nome cadastrado
            if (VerificaNomeCadastrado(Convert.ToInt32(txtCodigo.Text), txtNome.Text)) return;

            //Verifica se já tem algum registro com esse Cpf cadastrado
            if (VerificaCpfCadastrado(Convert.ToInt32(txtCodigo.Text), txtCpf.Text)) return;

            try
            {
                //Variável que vai conter o comando SQL para fazer a manipulação no banco de dados
                string sSql = "";

                //Verifica se o campo de Código está zerado para fazer um novo cadastro, ou se estiver preenchido faz a alteração do mesmo
                if (txtCodigo.Text == "0")
                {
                    //Monta o comando Insert para inserir o registro no banco de dados
                    sSql = "INSERT INTO tbl_Cliente(Nome, Cpf) VALUES('" + txtNome.Text.ToUpper().Trim() + "', '" + txtCpf.Text.Trim() + "');";

                    //Chama a função para fazer a manipulação no banco de dados passando o comando SQL montado acima
                    if (ExecutaManipulacao(sSql))
                    {
                        //Mostra a mensagem de Registro Inserido com Sucesso
                        MessageBox.Show("Registro Inserido com Sucesso!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    //Monta o comando Update para alterar o registro no banco de dados
                    sSql = "UPDATE tbl_Cliente SET Nome = '" + txtNome.Text + "', Cpf = '" + txtCpf.Text.ToUpper().Trim() + "' WHERE ClienteId = " + txtCodigo.Text.Trim() + ";";

                    //Chama a função para fazer a manipulação no banco de dados passando o comando SQL montado acima
                    if (ExecutaManipulacao(sSql))
                    {
                        //Mostra a mensagem de Registro Alterado com Sucesso
                        MessageBox.Show("Registro Alterado com Sucesso!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                //Pega o código atual para que se for uma atualização de registro não utilize a função de pegar o ultimo código inserido no banco
                int iCodAtual = Convert.ToInt32(txtCodigo.Text);

                //Atualiza o Grid com o Clientes Cadastrados
                CarregaGridCliente();

                if (iCodAtual == 0)
                {
                    //Preenche o campo Código com o Código Inserido no banco de dados
                    txtCodigo.Text = GetCodInseridoBanco().ToString();
                }
                else
                {
                    //Volta o código atual ao campo de código
                    txtCodigo.Text = iCodAtual.ToString();
                }
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro
                MessageBox.Show("Erro ao executar manipulação na tabela de clientes!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            //Se cancelar preenche novamente os campos com os dados corretos,
            //anulando qualquer edicação no campo pelo usuário
            PreencheCampos();
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            //Verifica se tem algum registro selecionado pelo campo código
            if (txtCodigo.Text == "" || txtCodigo.Text == "0")
            {
                //Mostra a mensagem para o usuário selecionar um registro para efetuar a exclusão
                MessageBox.Show("Nenhum registro selecionado para efetuar a exclusão!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Mensagem perguntando de o usuário deseja mesmo excluir o registro selecionado
            if (MessageBox.Show("Deseja mesmo excluir o cliente " + txtNome.Text, "Questão", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //Variável com o SQL para fazer a exclusão do cliente no banco de dados
                string sSql = "DELETE FROM tbl_Cliente WHERE ClienteId = " + txtCodigo.Text + ";";

                try
                {
                    //Faz a manipulação no banco de dados
                    if (ExecutaManipulacao(sSql))
                    {
                        //Mostra a mensagem de registro excluído com sucesso se não houve nenhum erro na manipulação
                        MessageBox.Show("Registro Excluído com Sucesso!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //Faz a atualização do grid limpando assim o registro excluído
                        CarregaGridCliente();
                    }
                }
                catch (Exception ex)
                {
                    //Caso de alguma erro mostra a mensagem de erro
                    MessageBox.Show("Erro ao excluir registro da tabela de clientes!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Limpa os controles de controle pai passado como parâmetro
        /// </summary>
        /// <param name="control">Controle pai onde os controles serão limpados</param>
        private void LimparCampos(Control control)
        {
            //Faz um loop em todos os controles do controle pai
            foreach (Control ctl in control.Controls)
            {
                //Verifica se o controle é um textBox, se for limpa o valor do mesmo
                if (ctl is TextBox) ctl.Text = "";
            }

            //Coloca o valo zero no campo de código caso o mesmo sejá limpo
            txtCodigo.Text = "0";
        }

        /// <summary>
        /// Função para pegar o ultimo código inserido no banco de dados
        /// </summary>
        /// <returns>Retorna um int com o último código que foi inserido no banco de dados</returns>
        private int GetCodInseridoBanco()
        {
            try
            {
                //Carrega a consulta do ultimo código inserido em um DataTable
                //Usa o bloco using para fazer a liberação automática dos recursos na memória ao fim da execução do mesmo
                using (DataTable dataTable = ExecutaConsulta("SELECT LAST_INSERT_ID() AS 'Codigo';"))
                {
                    //Verifica a consulta retornou dados fazendo a contagem de linhas do dataTable
                    if (dataTable.Rows.Count > 0)
                    {
                        //Faz a conversão e retorna a coluna código com o ultimo código inserido no banco de dados
                        return Convert.ToInt32(dataTable.Rows[0]["Codigo"]);
                    }
                }
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro
                MessageBox.Show("Erro ao consultar código inserido no banco de dados!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //Case de algum erro acima faz o retorno do valor 0
            return 0;
        }

        /// <summary>
        /// Verifica se o nome do cliente passado como parâmetro já existe no banco de dados
        /// </summary>
        /// <param name="iCodCliente">Código do cliente para que possa ser verificado um cliente com o mesmo nome mais de código diferente
        /// para que o banco não retorne o cliente atual que está sendo manipulado</param>
        /// <param name="sNomeCliente">Nome do cliente a ser verificado</param>
        /// <returns>Retorna [True] se encontrou o cliente com o nome no banco de dados e [False] caso não encontre</returns>
        private bool VerificaNomeCadastrado(int iCodCliente, string sNomeCliente)
        {
            try
            {
                //Cria o dataTable com os dados encontrados no banco de dados
                using (DataTable dataTable = ExecutaConsulta("SELECT ClienteId FROM tbl_Cliente WHERE Nome = '" + sNomeCliente.Trim() + "' AND ClienteId <> " + iCodCliente + ";"))
                {
                    //Conta as linhas do dataTable verificando quantos registros foram retornados
                    if (dataTable.Rows.Count > 0)
                    {
                        //Mostra a mensagem de que o nome já existe no banco de dados
                        MessageBox.Show("O nome " + txtNome.Text + " já existe no banco de dados!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        //Retorna true para indicar que o nome já existe no banco de dados
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro
                MessageBox.Show("Erro ao consultar nome já cadastrado no banco de dados!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //Retorna false caso não encontre nenhum registro com o nome indicado
            return false;
        }

        /// <summary>
        /// Verifica se o cpf do cliente passado como parâmetro já existe no banco de dados
        /// </summary>
        /// <param name="iCodCliente">Código do cliente para que possa ser verificado um cliente com o mesmo nome mais de código diferente
        /// para que o banco não retorne o cliente atual que está sendo manipulado</param>
        /// <param name="sCpf">Cpf do cliente a ser verificado</param>
        /// <returns>Retorna [True] se encontrou o cliente com o cpf no banco de dados e [False] caso não encontre</returns>
        private bool VerificaCpfCadastrado(int iCodCliente, string sCpf)
        {
            try
            {
                //Cria o dataTable com os dados encontrados no banco de dados
                using (DataTable dataTable = ExecutaConsulta("SELECT ClienteId FROM tbl_Cliente WHERE Cpf = '" + sCpf.Trim() + "' AND ClienteId <> " + iCodCliente + ";"))
                {
                    //Conta as linhas do dataTable verificando quantos registros foram retornados
                    if (dataTable.Rows.Count > 0)
                    {
                        //Mostra a mensagem de que o cpf já existe no banco de dados
                        MessageBox.Show("O cpf " + txtCpf.Text + " já existe no banco de dados!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        //Retorna true para indicar que o cpf já existe no banco de dados
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro
                MessageBox.Show("Erro ao consultar cpf já cadastrado no banco de dados!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //Retorna false caso não encontre nenhum registro com o cpf indicado
            return false;
        }

        /// <summary>
        /// Pega o click nas abas do tabControl
        /// </summary>
        /// <param name="sender">Objeto a ser manipulado</param>
        /// <param name="e">Evento a ser manipulado</param>
        private void tabControl1_Click(object sender, EventArgs e)
        {
            //Verifica se a tab selecionada é a 2 de cadastro e o campo código está vazio ou com o valor '0'
            if (tabControl1.SelectedTab == tabPage2 && txtCodigo.Text == "" || txtCodigo.Text == "0")
            {
                //Mostra a mensagem que para selecionar a tab de cadastro deve ter algum registro selecionado ou a inclusão de um novo pelo botão 'Novo'
                MessageBox.Show("Para visualizar os dados selecione um registro, ou aperte em Novo para incluir um novo registro!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Volta para a tab 1 de listagem de dados
                tabControl1.SelectTab(tabPage1);
            }
        }

        /// <summary>
        /// Preenche os campos de cadastro com o valor selecionado no dataGrid
        /// </summary>
        private void PreencheCampos()
        {
            try
            {
                //Verifica se o dataSource do dataGridView não é nulo ou se tem alguma linha de registro
                if (dataGridView1.DataSource != null && dataGridView1.Rows.Count > 0)
                {
                    //Passa os valores de cada célula correspondente na linha selecionada para o seu campo na aba de cadastro
                    txtCodigo.Text = dataGridView1.CurrentRow.Cells["ClienteId"].Value.ToString();
                    txtNome.Text   = dataGridView1.CurrentRow.Cells["Nome"].Value.ToString();
                    txtCpf.Text    = dataGridView1.CurrentRow.Cells["Cpf"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro
                MessageBox.Show("Erro ao carregar dados no Grid de Clientes!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Preenche os campos de cadastro com o registro selecionado com um click no grid
        /// </summary>
        /// <param name="sender">Objeto a ser manipulado</param>
        /// <param name="e">Evento a ser manipulado</param>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Preenche os campos com o registro selecionado
            PreencheCampos();
        }

        /// <summary>
        /// Preenche os campos de cadastro com o registro selecionado com dois click's no grid
        /// </summary>
        /// <param name="sender">Objeto a ser manipulado</param>
        /// <param name="e">Evento a ser manipulado</param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //Preenche os campos com o registro selecionado
            PreencheCampos();

            //Seleciona a tab de cadastro
            tabControl1.SelectTab(tabPage2);
        }

        /// <summary>
        /// Função que faz a pesquisa dos clientes no banco e os carrega bo dataGridView
        /// </summary>
        private void CarregaGridCliente()
        {
            //Faz as verificações dos campos de pesquisa preechidos e monta o Where para a pesquisa
            string sWhere = "";

            //Faz a verificação se o campo código do cliente na pesquisa está preenchido e o inclui na variável sWhere
            if (txtCodigoPesq.Text != "")
                sWhere = "ClienteId = " + txtCodigoPesq.Text;

            //Faz a verificação se o campo nome do cliente na pesquisa está preenchido e o inclui na variável sWhere
            if (txtNomePesq.Text != "")
            {
                //Verifica se a variável sWhere está vazia para inserir o valor nela, senão apenas adiciona ao valor já existente
                if (sWhere == "")
                    sWhere = "Nome LIKE '%" + txtNome.Text + "%'";
                else
                    sWhere = sWhere + " AND Nome LIKE '%" + txtNome.Text + "%'";
            }

            //Faz a verificação se o campo cpf do cliente na pesquisa está preenchido e o inclui na variável sWhere
            if (txtCpfPesq.Text != "")
            {
                //Verifica se a variável sWhere está vazia para inserir o valor nela, senão apenas adiciona ao valor já existente
                if (sWhere == "")
                    sWhere = "Cpf = '" + txtCpfPesq.Text + "'";
                else
                    sWhere = sWhere + " AND Cpf = '" + txtCpfPesq.Text + "'";
            }

            //SQL para a consulta na tabela tbl_Cliente
            var sSql = "SELECT * FROM tbl_Cliente";

            //Verifica se a variável sWhere não está vazia para adicionar o seu valor junto a variável sSql
            //para montar assim o comando SQL completo
            if (sWhere != "")
                sSql = sSql + " WHERE " + sWhere;

            try
            {
                //Limpa o dataSource do dataGridView o setando como null
                dataGridView1.DataSource = null;
                //Carrega o dataSource do dataGridView com o dataTable retornado na pesquisa
                dataGridView1.DataSource = ExecutaConsulta(sSql);

                //Caso o dataGridView tenha algum registro, seta a quantidade de registros no título do form
                if (dataGridView1.Rows.Count > 0)
                    this.Text = "Cadastro de Clientes - " + dataGridView1.Rows.Count + " Registro(s) Encontrado(s)";
                else
                {
                    //Se não tiver volta o título normal do form
                    this.Text = "Cadastro de Clientes";
                    //Mostra a mensagem de que não foi ecntroado nenhum registro para a seleção
                    MessageBox.Show("Nenhum registro encontrado para essa seleção!", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //Limpa os campos de pesquisa do form
                LimparCampos(this);
            }
            catch (Exception ex)
            {
                //Caso de alguma erro mostra a mensagem de erro e retorna [False]
                MessageBox.Show("Erro ao pesquisar clientes!\n\nErro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Permite digitar apenas número no campo de código
        /// </summary>
        /// <param name="sender">Objeto a ser manipulado</param>
        /// <param name="e">Evento a ser manipulado</param>
        private void txtCodigoPesq_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Verifica se o valor digitado é um número ou o BackSpace para apagar o dígito e se for permite o mesmo no campo
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }
    }
}