using Auth0.OidcClient;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Application.Services;
using TaskForge.Domain.Enums;
using TaskForge.WPF.ViewModels;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(
            Auth0Service auth0Service,
            IUserService userService,
            IProjectService projectService,
            ITaskFilterService filterService,
            ITaskService taskService,
            IExpenseService expenseService,
            IPasswordService passwordService,
            ISubscriptionService subscriptionService
         )
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(
                auth0Service,
                userService,
                projectService,
                filterService,
                taskService,
                expenseService,
                passwordService,
                subscriptionService
            );
        }
    }
}